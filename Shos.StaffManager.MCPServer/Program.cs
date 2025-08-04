using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Shos.StaffManager.Models;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services
       .AddMcpServer()
       .WithStdioServerTransport()
       .WithToolsFromAssembly();

await builder.Build().RunAsync();

[McpServerToolType]
public static class StaffManagerTools
{
    const string dataFilePath = @"C:\Users\G_KOJIMA_FUJIO\work\FC.StaffManager.json";
    static Company company = Company.Load(dataFilePath);

    [McpServerTool, Description("全部署の情報を取得")]
    public static Department[] GetAllDepartments() => company.DepartmentList.ToArray();

    [McpServerTool, Description("キーワードにヒットする部署の情報を取得")]
    public static Department[] SearchDepartments(string searchText) => company.GetDepartments(searchText: searchText).ToArray();

    [McpServerTool, Description("新たな部署の情報を追加")]
    public static bool AddNewDeparment(Department newDepartment)
    {
        try {
            company.DepartmentList.Add(newDepartment);
            company.Save(dataFilePath);
            return true;
        } catch {
            return false;
        }
    }

    [McpServerTool, Description("部署コードに該当する部署の情報を削除")]
    public static bool RemoveDeparmentWithCode(int departmentCode)
    {
        try {
            if (company.RemoveDepartment(departmentCode)) {
                company.Save(dataFilePath);
                return true;
            }
        } catch {
        }
        return false;
    }

    [McpServerTool, Description("全社員の情報を取得")]
    public static Staff[] GetAllStaffs() => company.StaffList.ToArray();

    [McpServerTool, Description("キーワードにヒットする社員の情報を取得")]
    public static Staff[] SearchStaffs(string searchText) => company.GetStaffs(searchText: searchText).ToArray();

    [McpServerTool, Description("新たな社員の情報を追加")]
    public static bool AddNewStaff(Staff newStaff)
    {
        try {
            company.StaffList.Add(newStaff);
            Save();
            return true;
        } catch {
            return false;
        }
    }

    [McpServerTool, Description("番号に該当する社員の情報を削除")]
    public static bool RemoveStaffWithNumbere(int number)
    {
        try {
            if (company.StaffList.Remove(number)) {
                company.Save(dataFilePath);
                return true;
            }
        } catch {
        }
        return false;
    }

    static void Save() =>company.Save(dataFilePath);
}

/*
npx @modelcontextprotocol/inspector dotnet run --project ./Shos.StaffManager.MCPServer/Shos.StaffManager.MCPServer.csproj

Visual Studio Code - 設定 - MCP - settings.json を編集

"servers": {
    "MCPServer.Console": {
        "type": "stdio",
        "command": "dotnet",
        "args": [
            "run",
            "--project",
            "C:\\[プロジェクト フォルダー]\\Shos.StaffManager.MCPServer.csproj"
        ]
    }
}

Claude Desktop - ファイル - 設定 - 開発者 - ローカルMCPサーバー - 設定を編集 - claude_desktop_config.json を編集

"mcpServers": {
    "StaffManagerTools": {
        "command": "[実行ファイル フォルダー]\\Shos.StaffManager.MCPServer.exe]"
    }
}
 */
