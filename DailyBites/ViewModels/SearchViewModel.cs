using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBites.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text.Json;

namespace DailyBites.ViewModels;

public partial class SearchViewModel : BaseViewModel
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http = new();

    [ObservableProperty] 
    private string _query = string.Empty;
    [ObservableProperty] 
    private ObservableCollection<UserResult> _results = new();
    [ObservableProperty] 
    private UserResult? _selectedUser;

    public SearchViewModel(IConfiguration config)
    {
        _config = config;
    }

    [RelayCommand]
    private async Task Search()
    {
        var q = (Query ?? string.Empty).Trim().ToLower();
        if (string.IsNullOrWhiteSpace(q))
        {
            Results.Clear();
            return;
        }

        Results.Clear();

        var projectId = _config["Firebase:ProjectId"];
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents:runQuery";

        // Prefix search: username >= q AND username < q + \uf8ff
        var end = q + "\uf8ff";

        var body = new
        {
            structuredQuery = new
            {
                from = new[] { new { collectionId = "users" } },
                where = new
                {
                    compositeFilter = new
                    {
                        op = "AND",
                        filters = new object[]
                        {
                            new
                            {
                                fieldFilter = new
                                {
                                    field = new { fieldPath = "username" },
                                    op = "GREATER_THAN_OR_EQUAL",
                                    value = new { stringValue = q }
                                }
                            },
                            new
                            {
                                fieldFilter = new
                                {
                                    field = new { fieldPath = "username" },
                                    op = "LESS_THAN_OR_EQUAL",
                                    value = new { stringValue = end }
                                }
                            }
                        }
                    }
                },
                limit = 25
            }
        };

        var res = await _http.PostAsJsonAsync(url, body);
        if (!res.IsSuccessStatusCode) return;

        using var json = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());

        foreach (var element in json.RootElement.EnumerateArray())
        {
            if (!element.TryGetProperty("document", out var doc)) continue;
            if (!doc.TryGetProperty("fields", out var fields)) continue;

            string GetString(string key)
            {
                return fields.TryGetProperty(key, out var f) && f.TryGetProperty("stringValue", out var sv)
                    ? sv.GetString() ?? string.Empty
                    : string.Empty;
            }

            var result = new UserResult
            {
                Uid = GetString("uid"),
                Username = GetString("username"),
                Name = GetString("name"),
                Email = GetString("email"),
                ProfilePicUrl = GetString("profilePicUrl")
            };

            if (!string.IsNullOrWhiteSpace(result.Username))
                Results.Add(result);
        }
    }

    partial void OnSelectedUserChanged(UserResult? value)
    {
        if (value is null) return;
        _ = Shell.Current.GoToAsync($"UserProfilePage?uid={Uri.EscapeDataString(value.Uid)}");
        SelectedUser = null;
    }
}
