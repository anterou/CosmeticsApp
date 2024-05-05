using Microsoft.Maui.Controls;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmeticsApp
{
    partial class AccountPage
    {
        public bool isAuth { get; set; }
        public string name { get; set; }
        public int UserId { get; set; }

        public AccountPage()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            if (isAuth == true)
            {
                StackLayout stackLayout = new StackLayout();
                Label label = new Label
                {
                    Text = "Вы вошли как " + name,
                    FontSize = 50,
                    HorizontalOptions = LayoutOptions.Center,
                };
                Button add_info = new Button
                {
                    Text = "Добавить информацию о себе",
                    HorizontalOptions = LayoutOptions.Center
                };
                Button button = new Button
                {
                    Text = "Вернуться на главную страницу",
                    HorizontalOptions = LayoutOptions.Center
                };
                Button leave_button = new Button
                {
                    Text = "Выйти из аккаунта",
                    HorizontalOptions = LayoutOptions.Center,
                };
                add_info.Clicked += add_info_Clicked;
                leave_button.Clicked += leaveAccount;
                button.Clicked += onBack;
                stackLayout.Children.Add(label);
                stackLayout.Children.Add(add_info);
                stackLayout.Children.Add(button);
                stackLayout.Children.Add(leave_button);
                stackLayout.Spacing = 20;
                Content = stackLayout;
            }
        }

        private async void leaveAccount(object sender, EventArgs e)
        {
            isAuth = false;
            onBack(sender, e);
        }

        private async void add_info_Clicked(object sender, EventArgs e)
        {
            string fullname = await DisplayPromptAsync("Question 1", "What's your full name?");
            string address = await DisplayPromptAsync("Question 2", "Where do you live?");
            int index = Convert.ToInt32(await DisplayPromptAsync("Question 3", "What's your mail index?"));
            try
            {
                var connectionstring = "Host=localhost;Username=postgres;Password=qwerty;Database=cosmetics;";
                using (var connection = new NpgsqlConnection(connectionstring))
                {
                    connection.Open();
                    string query = "INSERT INTO user_info (\"user\", full_name, address, \"index\"  ) VALUES (@user, @name, @address, @index);";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@user", await GetUserIdByNameAsync(name));
                        command.Parameters.AddWithValue("@name", fullname);
                        command.Parameters.AddWithValue("@address", address);
                        command.Parameters.AddWithValue("@index", index);
                        command.ExecuteNonQuery();
                    }
                }
                await DisplayAlert("Успех", "Информация добавлена", "OK");
            }
            catch
            {
                await DisplayAlert("Ошибка", "Что-то пошло не так. Вероятно, о вас уже присутствует информация", "OK");
            }
        }

        private async void onLogin(System.Object sender, System.EventArgs e)
        {
            var connectionstring = "Host=localhost;Username=postgres;Password=qwerty;Database=cosmetics;";
            string username = loginEntry.Text;
            string password = passwordEntry.Text;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Error", "Please enter username and password", "OK");
                return;
            }
            try
            {
                string database_pass = await GetUserPasswordAsync(username);
                if (password == database_pass)
                {
                    isAuth = true;
                    name = username;
                    await DisplayAlert("Success", "You are logged in", "OK");
                    OnAppearing();
                }
                else
                {
                    await DisplayAlert("Error", "Incorrect username or password", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        private async void onRegister(System.Object sender, System.EventArgs e)
        {
            string username = loginEntry.Text;
            string password = passwordEntry.Text;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Error", "Please enter username and password", "OK");
                return;
            }
            var max_query = "SELECT MAX(user_id) FROM users";
            var connectionstring = "Host=localhost;Username=postgres;Password=qwerty;Database=cosmetics;";
            using (var connection = new NpgsqlConnection(connectionstring))
            {
                await connection.OpenAsync();
                int max_id;
                using (var command_max = new NpgsqlCommand(max_query, connection))
                {
                    max_id = Convert.ToInt32(await command_max.ExecuteScalarAsync());
                }
                var query = $"INSERT INTO users (user_id, login, password, role) VALUES ({max_id + 1},'{username}', '{password}', 0)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                    await DisplayAlert("Success", "You are registered", "OK");
                }
            }
        }

        private async Task<string> GetUserPasswordAsync(string username)
        {
            var connectionstring = "Host=localhost;Username=postgres;Password=qwerty;Database=cosmetics;";
            var query = $"SELECT password FROM users WHERE login = '{username}'";
            using (var connection = new NpgsqlConnection(connectionstring))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    return result?.ToString();
                }
            }
        }

        private async void onBack(object sender, EventArgs e)
        {
            var mainPage = new MainPage();
            mainPage.isAuth = isAuth;
            mainPage.name = name;
            await Navigation.PushAsync(mainPage);
        }

        private async Task<int> GetUserIdByName(string userName)
        {
            string connString = "Host=localhost;Username=postgres;Password=qwerty;Database=cosmetics";
            using (var nc = new NpgsqlConnection(connString))
            {
                await nc.OpenAsync();
                string query = "SELECT * FROM get_user_id_by_login(@userName)";
                using (var command = new NpgsqlCommand(query, nc))
                {
                    command.Parameters.AddWithValue("@userName", userName);
                    var result = await command.ExecuteScalarAsync();
                    return result != null ? (int)result : -1;
                }
            }
        }

        private async Task<int> GetUserIdByNameAsync(string userName)
        {
            return await GetUserIdByName(userName);
        }
    }
}