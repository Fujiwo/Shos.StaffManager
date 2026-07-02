/// <summary>Main application namespace containing models, views, and controllers</summary>
namespace Shos.StaffManager.Application;

using Shos.StaffManager.Common.Helpers;
using Shos.StaffManager.Models;

/// <summary>Main program class that orchestrates the staff management application</summary>
class Program
{
    /// <summary>The file path for saving/loading company data</summary>
    const string dataFilePath = @"C:\work\FC.StaffManager.json";
    /// <summary>The name of the application</summary>
    const string applicationName = "FC.StaffManager";

    /// <summary>The company data model</summary>
    Company company = null!;
    /// <summary>The command manager for handling user operations</summary>
    Controllers.CommandManager operationManager = new();

    /// <summary>Runs the main application loop</summary>
    void Run()
    {
        if (!Load())
            return;

        ShowTitle();
        while (operationManager.Run(company))
            ;
        Save();
    }

    bool Load()
    {
        try {
            company = Company.Load(dataFilePath);
            return true;
        } catch (Exception ex) {
            UserInterface.ShowError($"データが読み込めませんでした。\n{ex.Message}");
            return false;
        }
    }

    bool Save()
    {
        try {
            company.Save(dataFilePath);
            return true;
        } catch (Exception ex) {
            UserInterface.ShowError($"データが書き込めませんでした。\n{ex.Message}");
            return false;
        }
    }

    /// <summary>Displays the application title</summary>
    static void ShowTitle() => UserInterface.Show($"<<{applicationName}>>");
        
    /// <summary>The main entry point of the application</summary>
    /// <param name="args">Command line arguments</param>
    static void Main(string[] args) => new Program().Run();
}
