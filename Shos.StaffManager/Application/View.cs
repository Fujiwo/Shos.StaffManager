
/// <summary>Contains view classes for displaying data</summary>
namespace Shos.StaffManager.Application.Views;

using Shos.Console;
using Shos.StaffManager.Models;

/// <summary>Provides methods for displaying company data in table format</summary>
class View
{
    /// <summary>Displays a collection of departments in table format</summary>
    /// <param name="departments">The departments to display</param>
    public static void Show(IEnumerable<Department> departments)
        => departments.OrderBy(department => department.Code)
                      .Select(department => new {
                          Code = $"{department.Code:D3}",
                          Name = department.Name
                      })
                      .ShowTable();

    /// <summary>Displays a collection of staff members in table format</summary>
    /// <param name="staffs">The staff members to display</param>
    public static void Show(IEnumerable<Staff> staffs)
        => staffs.OrderBy(staff => staff.Department.Code)
                 .ThenBy(staff => staff.Number)
                 .Select(staff => new {
                     Number = $"{staff.Number:D4}",
                     Name = $"{staff.Name}({staff.Ruby})",
                     Department = $"{staff.Department.Name}({staff.Department.Code})"
                 })
                 .ShowTable();

    /// <summary>Displays all departments in the company</summary>
    /// <param name="company">The company model</param>
    public static void ShowDepartments(Company company) => Show(company.DepartmentList);

    /// <summary>Displays all staff members in the company</summary>
    /// <param name="company">The company model</param>
    public static void ShowStaffs(Company company) => Show(company.StaffList);

    /// <summary>Displays staff members filtered by search text</summary>
    /// <param name="company">The company model</param>
    /// <param name="searchText">The text to search for in names, numbers, or departments</param>
    public static void ShowStaffs(Company company, string searchText)
        => Show(company.GetStaffs(searchText));
}
