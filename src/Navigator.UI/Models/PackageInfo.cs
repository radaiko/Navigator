using CommunityToolkit.Mvvm.ComponentModel;
using Navigator.UI.Utils;

namespace Navigator.UI.Models;

public partial class PackageInfo : ObservableObject {
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _version;
    [ObservableProperty] private string _description;
    [ObservableProperty] private string _license;
    [ObservableProperty] private string _url;

    public PackageInfo(Json json) {
        Name = json["name"].S;
        Version = json["version"].S;
        Description = json["description"].S;
        License = json["licenses"][0]["license"]["id"].S;
        Url = json["externalReferences"][0]["url"].S;
     }
 }
