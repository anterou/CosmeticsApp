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
        public AccountPage()
        {

            InitializeComponent();
                }
        protected override void OnAppearing()
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

                leave_button.Clicked += leaveAccount;
                button.Clicked += onBack;
                stackLayout.Children.Add(label);
                stackLayout.Children.Add(button);
                stackLayout.Children.Add(leave_button);
                stackLayout.Spacing = 20;
                Content = stackLayout;

            }
            void leaveAccount(object sender, EventArgs e)
            {
                isAuth = false;
                onBack(sender, e);
            }
        }
        



        async void onLogin(System.Object sender, System.EventArgs e)
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
                string database_pass = GetUserPasswordAsync(username).Result;
                if (password == database_pass)
                {
                    isAuth = true;
                    name = username;
                   
                    await DisplayAlert("Success", "You are logged in", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Incorrect username or password", "OK");
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }

        }
        async void onRegister(System.Object sender, System.EventArgs e)
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
                using(var command_max = new NpgsqlCommand(max_query, connection))
                {
                    max_id = Convert.ToInt32(await command_max.ExecuteScalarAsync());
                }
                var query = $"INSERT INTO users (user_id, login, password, role) VALUES ({max_id+1},'{username}', '{password}', 0)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                    await DisplayAlert("Success", "You are registered", "OK");
                }
            }
           

        }
        public async Task<string> GetUserPasswordAsync(string username)
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
    }
}
