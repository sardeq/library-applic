﻿// BorrowBooksPage.xaml.cs
using LibrarySystemWPF.Models;
using LibrarySystemWPF.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace LibrarySystemWPF.Pages
{
    public partial class BorrowBooksPage : Page
    {
        private readonly BookService _bookService = new BookService();
        private List<Book> _allBooks = new List<Book>();

        public BorrowBooksPage()
        {
            InitializeComponent();
            LoadAvailableBooks();
        }
        /*
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadAvailableBooks();
        }*/

        private void LoadAvailableBooks(string searchTerm = null)
        {
            if (UserSession.CurrentUser == null)
            {
                MessageBox.Show("You must be logged in to view available books.");
                NavigationService?.Navigate(new Uri("/Pages/LoginWindow.xaml", UriKind.Relative));
                return;
            }

            int userType = UserSession.CurrentUser.Type;
            _allBooks = _bookService.GetAvailableBooks(userType);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                _allBooks = _allBooks.Where(b =>
                    b.Title.ToLower().Contains(searchTerm) ||
                    b.Author.ToLower().Contains(searchTerm)
                ).ToList();
            }

            BooksGrid.ItemsSource = _allBooks.Select(b => new BookViewModel 
            {
                BookID = b.BookID,
                Title = b.Title,
                Author = b.Author,
                BooksAvailable = b.BooksAvailable,
                BorrowTypeDesc = b.BorrowType == 0 ? "Everyone" : "Teachers Only",
                BorrowDuration = b.BorrowDuration,
                IsSelected = false
            }).ToList();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAvailableBooks(SearchTextBox.Text);
        }

        private void BorrowButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBooks = BooksGrid.ItemsSource
                .Cast<dynamic>()
                .Where(b => b.IsSelected)
                .Select(b => (string)b.BookID)
                .ToList();

            if (selectedBooks.Count == 0)
            {
                MessageBox.Show("Please select at least one book to borrow.");
                return;
            }

            var (success, errors) = _bookService.BorrowBooks(UserSession.CurrentUser.ClientID, selectedBooks);
            if (success)
            {
                MessageBox.Show("Books borrowed successfully.");
                LoadAvailableBooks(); 
            }
            else
            {
                string errorMessage = string.Join("\n", errors);
                MessageBox.Show($"Borrowing failed:\n{errorMessage}");
            }
        }
    }
}