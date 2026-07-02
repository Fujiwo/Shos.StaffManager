using Shos.StaffManager.Models;
using StaffManager.Application.Views;
using StaffManager.Common.ControllersBase;
using System.ComponentModel;

namespace StaffManager.AI;

static class Toolbox
{
    public static Company? Company { get; set; }

    [Description("Retrieve all departments")]
    public static Department[] GetAllDepartments() => Company is null ? [] :  Company.DepartmentList.ToArray();

    [Description("Search departments by keyword")]
    public static Department[] SearchDepartments(string keyword) => Company is null ? [] : Company.GetDepartments(searchText: keyword).ToArray();

    [Description("Add a new department")]
    public static bool AddNewDepartment(Department newDepartment)
    {
        try {
            if (Company is not null) {
                Company.DepartmentList.Add(newDepartment);
                return true;
            }
        } catch {
        }
        return false;
    }

    [Description("Remove a department by code")]
    public static bool RemoveDepartmentByCode(int departmentCode)
    {
        try {
            if (Company is not null && Company.RemoveDepartment(departmentCode))
                return true;
        } catch {
        }
        return false;
    }

    [Description("Change a department's name by department code")]
    public static bool ChangeDepartmentNameByCode(int departmentCode, string newName)
    {
        try {
            if (Company is not null && Company.ChangeDepartmentName(departmentCode, newName))
                return true;
        } catch {
        }
        return false;
    }

    [Description("Retrieve all staff members")]
    public static Staff[] GetAllStaffs() => Company is null ? [] : Company.StaffList.ToArray();

    [Description("Search staff members by keyword")]
    public static Staff[] SearchStaffs(string keyword) => Company is null ? [] : Company.GetStaffs(searchText: keyword).ToArray();

    [Description("Add a new staff member")]
    public static bool AddNewStaff(Staff newStaff)
    {
        try {
            if (Company is not null) {
                Company.StaffList.Add(newStaff);
                return true;
            }
        } catch {
        }
        return false;
    }

    [Description("Remove a staff member by number")]
    public static bool RemoveStaffbyNumber(int number)
    {
        try {
            if (Company is not null && Company.StaffList.Remove(number))
                return true;
        } catch {
        }
        return false;
    }

    [Description("Change a staff member's department by staff number")]
    public static bool ChangeStaffsDepartmentByNumber(int number, int newDepartmentCode)
    {
        try {
            if (Company is not null && Company.ChangeStaffsDepartment(number, newDepartmentCode))
                return true;
        } catch {
        }
        return false;
    }

    [Description("Show all departments")]
    public static void ShowAllDepartments()
    {
        if (Company is not null)
            View.ShowDepartments(Company);
    }

    [Description("Show all staffs")]
    public static void ShowAllStaffs()
    {
        if (Company is not null)
            View.ShowStaffs(Company);
    }
}
