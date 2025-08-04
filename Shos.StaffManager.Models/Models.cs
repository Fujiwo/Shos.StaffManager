using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shos.StaffManager.Models
{
    /// <summary>Exception thrown when serialization/deserialization fails</summary>
    public class SerializeException : ApplicationException
    {
        /// <summary>Initializes a new instance of the SerializeException class</summary>
        /// <param name="message">The error message</param>
        public SerializeException(string message) : base(message)
        {}
    }

    /// <summary>Represents a department with code and name</summary>
    /// <param name="Code">The department code (100-999)</param>
    /// <param name="Name">The department name (1-30 characters)</param>
    public record Department(int Code, string Name)
    {
        /// <summary>Minimum allowed department code</summary>
        public const int MinimumCode       = 100;
        /// <summary>Maximum allowed department code</summary>
        public const int MaximumCode       = 999;
        /// <summary>Minimum allowed name length</summary>
        public const int MinimumNameLength =   1;
        /// <summary>Maximum allowed name length</summary>
        public const int MaximumNameLength =  30;
    }

    /// <summary>Represents a staff member with number, name, ruby reading, and department</summary>
    /// <param name="Number">The staff number (1-9999)</param>
    /// <param name="Name">The staff name (1-30 characters)</param>
    /// <param name="Ruby">The phonetic reading of the name</param>
    /// <param name="Department">The department the staff belongs to</param>
    public record Staff(int Number, string Name, string Ruby, Department Department)
    {
        /// <summary>Minimum allowed staff number</summary>
        public const int MinimumNumber     =    1;
        /// <summary>Maximum allowed staff number</summary>
        public const int MaximumNumber     = 9999;
        /// <summary>Minimum allowed name length</summary>
        public const int MinimumNameLength =    1;
        /// <summary>Maximum allowed name length</summary>
        public const int MaximumNameLength =   30;
    }

    /// <summary>Serializable version of Staff record that stores department as code instead of object</summary>
    /// <param name="Number">The staff number</param>
    /// <param name="Name">The staff name</param>
    /// <param name="Ruby">The phonetic reading</param>
    /// <param name="DepartmentCode">The department code</param>
    public record SerializableStaff(int Number, string Name, string Ruby, int DepartmentCode)
    {
        /// <summary>Creates a SerializableStaff from a Staff object</summary>
        /// <param name="staff">The staff object to convert</param>
        /// <returns>A serializable staff representation</returns>
        public static SerializableStaff From(Staff staff)
            => new SerializableStaff(Number: staff.Number, Name: staff.Name, Ruby: staff.Ruby, DepartmentCode: staff.Department.Code);

        /// <summary>Converts this SerializableStaff back to a Staff object</summary>
        /// <param name="departmentList">The department list to resolve the department</param>
        /// <returns>A Staff object</returns>
        /// <exception cref="SerializeException">Thrown when department code is not found</exception>
        public Staff To(DepartmentList departmentList)
        {
            var department = departmentList.FirstOrDefault(department => department.Code == DepartmentCode);
            if (department is null)
                throw new SerializeException("There is no corresponding department code.");
            return new Staff(Number: Number, Name: Name, Ruby: Ruby, Department: department);
        }
    }

    /// <summary>A collection of departments that implements IEnumerable</summary>
    public class DepartmentList : IEnumerable<Department>
    {
        /// <summary>Internal list that stores the department objects</summary>
        List<Department> departments = new();

        /// <summary>Gets the department at the specified index</summary>
        /// <param name="index">The index of the department</param>
        /// <returns>The department at the specified index</returns>
        public Department this[int index] => departments[index];

        /// <summary>Adds a department to the list</summary>
        /// <param name="department">The department to add</param>
        public void Add(Department department) => departments.Add(department);

        /// <summary>Remove a department from the list</summary>
        /// <param name="code">The code of department to remove</param>
        public bool Remove(int code)
        {
            var foundDepartment = departments.FirstOrDefault(department => department.Code == code);
            if (foundDepartment is null)
                return false;
            departments.Remove(foundDepartment);
            return true;
        }

        /// <summary>Returns an enumerator for the departments</summary>
        /// <returns>An enumerator for Department objects</returns>
        public IEnumerator<Department> GetEnumerator() => departments.GetEnumerator();

        /// <summary>Returns a non-generic enumerator</summary>
        /// <returns>An enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>A collection of staff members that implements IEnumerable</summary>
    public class StaffList : IEnumerable<Staff>
    {
        /// <summary>Internal list that stores the staff member objects</summary>
        List<Staff> staffs = new();

        /// <summary>Adds a staff member to the list</summary>
        /// <param name="staff">The staff member to add</param>
        public void Add(Staff staff) => staffs.Add(staff);

        /// <summary>Remove a staff from the list</summary>
        /// <param name="number">The number of staff to remove</param>
        public bool Remove(int number)
        {
            var foundStaff = staffs.FirstOrDefault(staff => staff.Number == number);
            if (foundStaff is null)
                return false;
            staffs.Remove(foundStaff);
            return true;
        }

        /// <summary>Returns an enumerator for the staff members</summary>
        /// <returns>An enumerator for Staff objects</returns>
        public IEnumerator<Staff> GetEnumerator() => staffs.GetEnumerator();

        /// <summary>Returns a non-generic enumerator</summary>
        /// <returns>An enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Represents a company containing departments and staff members</summary>
    public class Company
    {
        /// <summary>JSON serialization options for pretty printing</summary>
        static readonly JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };

        /// <summary>Gets or sets the version number of the application data format</summary>
        public string Version { get; set; } = "0.1";

        /// <summary>Gets the list of departments (excluded from JSON serialization)</summary>
        [JsonIgnore]
        public DepartmentList DepartmentList { get; private set; } = new();
        /// <summary>Gets the list of staff members (excluded from JSON serialization)</summary>
        [JsonIgnore]
        public StaffList StaffList { get; private set; } = new();

        /// <summary>Gets or sets the serializable department list for JSON operations</summary>
        [JsonInclude]
        Department[] SerializableDepartmentList
        {
            get => DepartmentList.ToArray();
            set => DepartmentList = [.. value];
        }

        /// <summary>Gets or sets the serializable staff list for JSON operations</summary>
        [JsonInclude]
        IEnumerable<SerializableStaff> SerializableStaffList
        {
            get => StaffList.Select(staff => SerializableStaff.From(staff));
            set => StaffList = [.. value.Select(serializableStaff => serializableStaff.To(DepartmentList))];
        }

        /// <summary>Initializes a new instance of the Company class</summary>
        public Company()
        {}

        /// <summary>Initializes a new instance of the Company class for JSON deserialization</summary>
        /// <param name="departmentList">The department list</param>
        /// <param name="staffList">The staff list</param>
        [JsonConstructor]
        public Company(DepartmentList departmentList, StaffList staffList)
            => (DepartmentList, StaffList) = (departmentList, staffList);

        /// <summary>Departments filtered by search text</summary>
        /// <param name="searchText">The text to search for in names or codes</param>
        public IEnumerable<Department> GetDepartments(string searchText = "")
            => DepartmentList.Where(department => department.Name.Contains(searchText) ||
                                                  department.Code.ToString().Equals(searchText));

        /// <summary>Staff members filtered by search text</summary>
        /// <param name="searchText">The text to search for in names, numbers, or departments</param>
        public IEnumerable<Staff> GetStaffs(string searchText = "")
            => StaffList.Where(staff => staff.Name.Contains(searchText)            ||
                                        staff.Number.ToString().Equals(searchText) ||
                                        staff.Department.Name.Contains(searchText));

        /// <summary>Remove a department</summary>
        /// <param name="code">The code of department to remove</param>
        public bool RemoveDepartment(int departmentCode)
            => StaffList.Any(staff => staff.Department.Code == departmentCode)
               ? false : DepartmentList.Remove(departmentCode);

        /// <summary>Saves the company data to a JSON file</summary>
        /// <param name="filePath">The path to save the file</param>
        public void Save(string filePath)
        {
            var jsonString = JsonSerializer.Serialize(this, jsonSerializerOptions);
            File.WriteAllText(path: filePath, contents: jsonString, encoding: Encoding.UTF8);
        }

        /// <summary>Loads company data from a JSON file</summary>
        /// <param name="filePath">The path to the file to load</param>
        /// <returns>A Company object or a new instance if loading fails</returns>
        public static Company Load(string filePath)
        {
            if (!File.Exists(filePath))
                return new Company();
            var jsonString = File.ReadAllText(path: filePath, encoding: Encoding.UTF8);
            return JsonSerializer.Deserialize<Company>(jsonString) ?? new Company();
        }
    }
}
