﻿using KoOrderRegister.Localization;
using KoOrderRegister.Modules.Customer.Pages;
using KoOrderRegister.Modules.Database.Models;
using KoOrderRegister.Modules.Database.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KoOrderRegister.Modules.Customer.ViewModels
{
    public class CustomerListViewModel : INotifyPropertyChanged
    {
        private readonly IDatabaseModel _database;
        private PersonDetailsPage _personDetailsPage;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (TargetInvocationException ex)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
            }

        }
        #region Commands
        public ICommand UpdateCommand => new Command(Update);
        public ICommand AddNewCustomerCommand => new Command(AddNewCustomer);
        public Command<CustomerModel> DeleteCustomerCommand => new Command<CustomerModel>(DeleteCustomer);
        public Command<CustomerModel> EditCustomerCommand => new Command<CustomerModel>(EditCustumer);
        #endregion
        public ObservableCollection<CustomerModel> Customers { get; set; } = new ObservableCollection<CustomerModel>();
        public CustomerListViewModel(IDatabaseModel database, PersonDetailsPage personDetailsPage)
        {
            _database = database;
            _personDetailsPage = personDetailsPage;

            Update();
        }

        public async void Update()
        {
            if(Customers != null)
            {
                Customers.Clear();
            }
            foreach(var customer in await _database.GetAllCustomers())
            {
                Customers.Add(customer);
            }
        }

        public async void AddNewCustomer()
        {
            await App.Current.MainPage.Navigation.PushAsync(_personDetailsPage);
        }

        public async void EditCustumer(CustomerModel customer) 
        {
            _personDetailsPage.EditCustomer(customer);
            await App.Current.MainPage.Navigation.PushAsync(_personDetailsPage);
        }
        public async void DeleteCustomer(CustomerModel customer) 
        {
            bool result = await Application.Current.MainPage.DisplayAlert(AppRes.Delete, AppRes.AreYouSureYouWantToDelete + " " + customer.Name, AppRes.Yes, AppRes.No);
            if (result)
            {
               int deleteResult = await _database.DeleteCustomer(customer.Guid);
                if(deleteResult == 1)
                {
                    Customers.Remove(customer);
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(AppRes.Delete, AppRes.FailedToDelete + " " + customer.Name, AppRes.Ok);
                }
            }
        }

    }
}
