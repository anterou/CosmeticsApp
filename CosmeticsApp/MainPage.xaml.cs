using Npgsql;
using System.Data;
using System.Security.Cryptography.X509Certificates;

namespace CosmeticsApp
{
    public partial class MainPage : ContentPage
    {
        public bool isAuth { get; set; }
        public string name { get; set; }
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
                Grid grid = new Grid
                {
                    RowDefinitions = new RowDefinitionCollection
                    {
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    },
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                    }
                };

                int row = 0;
                int col = 0;
                while (reader.Read())
                {
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
                                }
                            }
                        }
                    };

                    Grid.SetRow(productFrame, row);
                    Grid.SetColumn(productFrame, col);
                    grid.Children.Add(productFrame);

                    col++;
                    if (col == 2)
                    {
                        row++;
                        col = 0;
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
    }
}