using System.Net.Http.Json;
using ServiceDesk.Shared.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Devices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

namespace ServiceDesk.Maui.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        public ApiService()
        {
            _httpClient = new HttpClient(GetInsecureHandler());

            string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:5172/"
                : "https://localhost:7249/";

            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        private HttpMessageHandler GetInsecureHandler()
        {
#if ANDROID
            var handler = new Xamarin.Android.Net.AndroidMessageHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            return handler;
#else
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            return handler;
#endif
        }

        public async Task<List<Technician>> GetTechniciansAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<List<Technician>>("api/technicians", _jsonOptions);
            return data ?? new List<Technician>();
        }
        public async Task<bool> AddTechnicianAsync(Technician technician)
        {
            try { return (await _httpClient.PostAsJsonAsync("api/technicians", technician, _jsonOptions)).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> UpdateTechnicianAsync(Technician technician)
        {
            try { return (await _httpClient.PutAsJsonAsync($"api/technicians/{technician.Id}", technician, _jsonOptions)).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> DeleteTechnicianAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/technicians/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    string errorDetail = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Server returned {response.StatusCode}. Details: {errorDetail}");
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<WorkOrder>> GetWorkOrdersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/workorders");
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<WorkOrder>>(jsonString, _jsonOptions) ?? new List<WorkOrder>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot parse Tasks: {ex.Message}");
            }
        }
        public async Task<bool> CreateWorkOrderAsync(WorkOrder workOrder)
        {
            try { return (await _httpClient.PostAsJsonAsync("api/workorders", workOrder, _jsonOptions)).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> UpdateWorkOrderAsync(WorkOrder workOrder)
        {
            try { return (await _httpClient.PutAsJsonAsync($"api/workorders/{workOrder.Id}", workOrder, _jsonOptions)).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> DeleteWorkOrderAsync(int id)
        {
            try { return (await _httpClient.DeleteAsync($"api/workorders/{id}")).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> AddWorkOrderAsync(WorkOrder order)
        {
            try { return (await _httpClient.PostAsJsonAsync("api/workorders", order)).IsSuccessStatusCode; } catch { return false; }
        }

        public async Task<List<Customer>> GetCustomersAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<List<Customer>>("api/customers", _jsonOptions);
            return data ?? new List<Customer>();
        }
        public async Task<bool> AddCustomerAsync(Customer customer)
        {
            try { return (await _httpClient.PostAsJsonAsync("api/customers", customer, _jsonOptions)).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            try { return (await _httpClient.PutAsJsonAsync($"api/customers/{customer.Id}", customer, _jsonOptions)).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            try { return (await _httpClient.DeleteAsync($"api/customers/{id}")).IsSuccessStatusCode; } catch { return false; }
        }

        public async Task<List<Equipment>> GetEquipmentAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<List<Equipment>>("api/equipment", _jsonOptions);
            return data ?? new List<Equipment>();
        }
        public async Task<bool> AddEquipmentAsync(Equipment equipment)
        {
            try { return (await _httpClient.PostAsJsonAsync("api/equipment", equipment, _jsonOptions)).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> UpdateEquipmentAsync(Equipment equipment)
        {
            try { return (await _httpClient.PutAsJsonAsync($"api/equipment/{equipment.Id}", equipment, _jsonOptions)).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> DeleteEquipmentAsync(int id)
        {
            try { return (await _httpClient.DeleteAsync($"api/equipment/{id}")).IsSuccessStatusCode; } catch { return false; }
        }

        public async Task<List<InventoryItem>> GetInventoryAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<List<InventoryItem>>("api/inventory", _jsonOptions);
            return data ?? new List<InventoryItem>();
        }
        public async Task<bool> AddInventoryItemAsync(InventoryItem item)
        {
            try { return (await _httpClient.PostAsJsonAsync("api/inventory", item, _jsonOptions)).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> UpdateInventoryItemAsync(InventoryItem item)
        {
            try { return (await _httpClient.PutAsJsonAsync($"api/inventory/{item.Id}", item, _jsonOptions)).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> DeleteInventoryItemAsync(int id)
        {
            try { return (await _httpClient.DeleteAsync($"api/inventory/{id}")).IsSuccessStatusCode; } catch { return false; }
        }

        public async Task<List<Supplier>> GetSuppliersAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<List<Supplier>>("api/suppliers", _jsonOptions);
            return data ?? new List<Supplier>();
        }
        public async Task<bool> AddSupplierAsync(Supplier supplier)
        {
            try { return (await _httpClient.PostAsJsonAsync("api/suppliers", supplier, _jsonOptions)).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> UpdateSupplierAsync(Supplier supplier)
        {
            try { return (await _httpClient.PutAsJsonAsync($"api/suppliers/{supplier.Id}", supplier, _jsonOptions)).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> DeleteSupplierAsync(int id)
        {
            try { return (await _httpClient.DeleteAsync($"api/suppliers/{id}")).IsSuccessStatusCode; } catch { return false; }
        }

        public async Task<List<Purchase>> GetPurchasesAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<List<Purchase>>("api/purchases", _jsonOptions);
            return data ?? new List<Purchase>();
        }
        public async Task<bool> AddPurchaseAsync(Purchase purchase)
        {
            try { return (await _httpClient.PostAsJsonAsync("api/purchases", purchase, _jsonOptions)).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> UpdatePurchaseAsync(Purchase purchase)
        {
            try { return (await _httpClient.PutAsJsonAsync($"api/purchases/{purchase.Id}", purchase, _jsonOptions)).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> DeletePurchaseAsync(int id)
        {
            try { return (await _httpClient.DeleteAsync($"api/purchases/{id}")).IsSuccessStatusCode; } catch { return false; }
        }

        public async Task<List<Quotation>> GetQuotationsAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<List<Quotation>>("api/quotations", _jsonOptions);
            return data ?? new List<Quotation>();
        }

        public async Task<bool> AddQuotationAsync(Quotation quotation)
        {
            var response = await _httpClient.PostAsJsonAsync("api/quotations", quotation, _jsonOptions);
            if (!response.IsSuccessStatusCode)
            {
                string errorDetail = await response.Content.ReadAsStringAsync();
                throw new Exception($"Server Error: {errorDetail}");
            }
            return true;
        }

        public async Task<bool> UpdateQuotationAsync(Quotation quotation)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/quotations/{quotation.Id}", quotation, _jsonOptions);
            if (!response.IsSuccessStatusCode)
            {
                string errorDetail = await response.Content.ReadAsStringAsync();
                throw new Exception($"Server Error: {errorDetail}");
            }
            return true;
        }

        public async Task<bool> DeleteQuotationAsync(int id)
        {
            try { return (await _httpClient.DeleteAsync($"api/quotations/{id}")).IsSuccessStatusCode; } catch { return false; }
        }

        public async Task<List<Invoice>> GetInvoicesAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<List<Invoice>>("api/invoices", _jsonOptions);
            return data ?? new List<Invoice>();
        }
        public async Task<bool> AddInvoiceAsync(Invoice invoice)
        {
            try { return (await _httpClient.PostAsJsonAsync("api/invoices", invoice, _jsonOptions)).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> UpdateInvoiceAsync(Invoice invoice)
        {
            try { return (await _httpClient.PutAsJsonAsync($"api/invoices/{invoice.Id}", invoice, _jsonOptions)).IsSuccessStatusCode; } catch { return false; }
        }
        public async Task<bool> DeleteInvoiceAsync(int id)
        {
            try { return (await _httpClient.DeleteAsync($"api/invoices/{id}")).IsSuccessStatusCode; } catch { return false; }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            var response = await _httpClient.GetAsync("api/users");

            if (!response.IsSuccessStatusCode)
            {
                string errorDetail = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error ({response.StatusCode}): {errorDetail}");
            }

            var data = await response.Content.ReadFromJsonAsync<List<User>>(_jsonOptions);
            return data ?? new List<User>();
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> UpdateUserAsync(User user)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/users/{user.Id}", user, _jsonOptions);
                if (response.IsSuccessStatusCode)
                {
                    return (true, string.Empty);
                }

                string errorDetail = await response.Content.ReadAsStringAsync();
                return (false, $"Server returned {response.StatusCode}. Details: {errorDetail}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}