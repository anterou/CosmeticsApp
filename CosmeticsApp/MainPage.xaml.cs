using Npgsql;
using System.Data;
using System.Security.Cryptography.X509Certificates;

namespace CosmeticsApp
{
    public partial class MainPage : ContentPage
    {
        public bool isAuth { get; set; }
        public string name { get; set; }
        private Frame selectedProductFrame;

        public MainPage()
        {
            InitializeComponent();

            string connString = "Host=localhost;Username=postgres;Password=qwerty;Database=cosmetics";
            NpgsqlConnection nc = new NpgsqlConnection(connString);

            try
            {
                // Открываем соединение
                nc.Open();
                Console.WriteLine("Соединение установлено");
                // Другие операции с базой данных
            }
            catch (Exception ex)
            {
                // Обработка ошибки
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            if (nc.FullState == ConnectionState.Open)
            {
                string query = "SELECT production.name, categories.name, price from production inner join categories on production.category = categories.category_id";
                using var command = new NpgsqlCommand(query, nc);
                using var reader = command.ExecuteReader();

                int maxRows = 20; // Максимальное число строк
                int maxCols = 2; // Максимальное число столбцов
                int currentRow = 0;
                int currentCol = 0;

                Grid grid = new Grid
                {
                    RowDefinitions = new RowDefinitionCollection(),
                    ColumnDefinitions = new ColumnDefinitionCollection()
                };

                for (int i = 0; i < maxRows; i++)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                }

                for (int i = 0; i < maxCols; i++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                while (reader.Read())
                {
                    Button buyButton = new Button
                    {
                        Text = "Купить",
                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Button)),
                        HorizontalOptions = LayoutOptions.Start,
                        BackgroundColor = Color.FromRgb(205, 92, 92)
                    };

                    buyButton.Clicked += async (sender, e) =>
                    {
                        if (isAuth)
                        {
                            selectedProductFrame = (Frame)((Button)sender).Parent.Parent;
                            await HandleConfirmButtonClick();
                        }
                        else await DisplayAlert("Ошибка", "Для покупки необходимо авторизоваться", "OK");
                    };

                    var productFrame = new Frame
                    {
                        Margin = new Thickness(15),
                        Padding = new Thickness(5),
                        BackgroundColor = Color.FromRgb(105, 105, 105),
                        CornerRadius = 5,
                        Content = new StackLayout
                        {
                            Children =
                            {
                                new Label
                                {
                                    Text = reader.GetString(0), // Получение названия товара
                                    FontAttributes = FontAttributes.Bold,
                                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                                    LineBreakMode = LineBreakMode.NoWrap,
                                    HorizontalOptions = LayoutOptions.StartAndExpand
                                },
                                new Label
                                {
                                    Text = reader.GetString(1), // Получение категории товара
                                    FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                                    LineBreakMode = LineBreakMode.WordWrap,
                                    HorizontalOptions = LayoutOptions.StartAndExpand
                                },
                                new Label
                                {
                                    Text = Math.Round(reader.GetDouble(2), 2).ToString() + "$", // Получение цены
                                    FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                                    LineBreakMode = LineBreakMode.WordWrap,
                                    HorizontalOptions = LayoutOptions.StartAndExpand
                                },
                                buyButton
                            }
                        }
                    };

                    Grid.SetRow(productFrame, currentRow);
                    Grid.SetColumn(productFrame, currentCol);
                    grid.Children.Add(productFrame);

                    currentCol++;
                    if (currentCol == maxCols)
                    {
                        currentRow++;
                        currentCol = 0;
                    }
                }

                nc.Close();
                MainStack.Children.Add(grid);
            }
        }

        private async void onLoginButton(object sender, EventArgs e)
        {
            var accountPage = new AccountPage();
            accountPage.isAuth = isAuth;
            accountPage.name = name;
            await Navigation.PushAsync(accountPage);
        }

        async Task HandleConfirmButtonClick()
        {
            string selectedProductName = GetProductNameFromFrame(selectedProductFrame);
            int selectedProductPrice = GetProductPriceFromFrame(selectedProductFrame);
            if (!string.IsNullOrEmpty(selectedProductName))
            {
                int userId = await GetUserIdByName(name);
                int productId = await GetProductIdByName(selectedProductName);

                if (userId != -1 && productId != -1)
                {
                    // Сохраняем информацию о покупке в базе данных
                    await SavePurchaseAsync(userId, productId, selectedProductPrice);
                    await DisplayAlert("Покупка", $"Вы приобрели товар: {selectedProductName}", "OK");
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось определить идентификатор пользователя или товара", "OK");
                }
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось определить название товара", "OK");
            }
        }

        private async Task SavePurchaseAsync(int userId, int productId, int price)
        {
            string connString = "Host=localhost;Username=postgres;Password=qwerty;Database=cosmetics";
            using (var nc = new NpgsqlConnection(connString))
            {
                await nc.OpenAsync();
                string query = "INSERT INTO transactions (\"user\", product, date, cost) VALUES (@userId, @productId, @purchaseDate, @cost)";
                using var command = new NpgsqlCommand(query, nc);
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@productId", productId);
                command.Parameters.AddWithValue("@purchaseDate", DateTime.Now);
                command.Parameters.AddWithValue("@cost", price);
                await command.ExecuteNonQueryAsync();
            }
        }

        private int GetProductPriceFromFrame(Frame frame)
        {
            if (frame.Content is StackLayout stackLayout)
            {
                var productPriceLabel = stackLayout.Children.OfType<Label>().LastOrDefault();
                if (productPriceLabel != null)
                {
                    return Convert.ToInt32(productPriceLabel.Text.Substring(0, productPriceLabel.Text.Length - 1));
                }
            }
            return 0;
        }

        private string GetProductNameFromFrame(Frame frame)
        {
            if (frame.Content is StackLayout stackLayout)
            {
                var productNameLabel = stackLayout.Children.OfType<Label>().FirstOrDefault();
                if (productNameLabel != null)
                {
                    return productNameLabel.Text;
                }
            }
            return string.Empty;
        }

        private async Task<int> GetUserIdByName(string userName)
        {
            string connString = "Host=localhost;Username=postgres;Password=qwerty;Database=cosmetics";
            using (var nc = new NpgsqlConnection(connString))
            {
                await nc.OpenAsync();
                string query = "SELECT user_id FROM users WHERE login = @userName";
                using var command = new NpgsqlCommand(query, nc);
                command.Parameters.AddWithValue("@userName", userName);
                var result = await command.ExecuteScalarAsync();
                return result != null ? (int)result : -1;
            }
        }

        private async Task<int> GetProductIdByName(string productName)
        {
            string connString = "Host=localhost;Username=postgres;Password=qwerty;Database=cosmetics";
            using (var nc = new NpgsqlConnection(connString))
            {
                await nc.OpenAsync();
                string query = "SELECT product_id FROM production WHERE name = @productName";
                using var command = new NpgsqlCommand(query, nc);
                command.Parameters.AddWithValue("@productName", productName);
                var result = await command.ExecuteScalarAsync();
                return result != null ? (int)result : -1;
            }
        }
    }
}
