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


namespace Shos.StaffManager.MCPServer
{
    [McpServerToolType]
    public static class StaffManagerTools
    {
        const string dataFilePath = @"C:\work\FC.StaffManager.json";
        static Company company = Company.Load(dataFilePath);

        [McpServerTool, Description("Retrieve all departments")]
        public static Department[] GetAllDepartments() => company.DepartmentList.ToArray();

        [McpServerTool, Description("Search departments by keyword")]
        public static Department[] SearchDepartments(string keyword) => company.GetDepartments(searchText: keyword).ToArray();

        [McpServerTool, Description("Add a new department")]
        public static bool AddNewDepartment(Department newDepartment)
        {
            try {
                company.DepartmentList.Add(newDepartment);
                company.Save(dataFilePath);
                return true;
            } catch {
                return false;
            }
        }

        [McpServerTool, Description("Remove a department by code")]
        public static bool RemoveDepartmentByCode(int departmentCode)
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

        [McpServerTool, Description("Retrieve all staff members")]
        public static Staff[] GetAllStaffs() => company.StaffList.ToArray();

        [McpServerTool, Description("Search staff members by keyword")]
        public static Staff[] SearchStaffs(string keyword) => company.GetStaffs(searchText: keyword).ToArray();

        [McpServerTool, Description("Add a new staff member")]
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

        [McpServerTool, Description("Remove a staff member by number")]
        public static bool RemoveStaffbyNumber(int number)
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
}

/*
npx @modelcontextprotocol/inspector dotnet run --project ./Shos.StaffManager.MCPServer/Shos.StaffManager.MCPServer.csproj

Visual Studio Code - Settings - MCP - Edit "settings.json"

"servers": {
    "MCPServer.Console": {
        "type": "stdio",
        "command": "dotnet",
        "args": [
            "run",
            "--project",
            "C:\\[Project Folder]\\Shos.StaffManager.MCPServer.csproj"
        ]
    }
}

Claude Desktop - File - Settings - Developer - Local MCP server - Edit Settings - Edit "claude_desktop_config.json"

"mcpServers": {
    "StaffManagerTools": {
        "command": "[Executable File Folder]\\Shos.StaffManager.MCPServer.exe]"
    }
}
 */
