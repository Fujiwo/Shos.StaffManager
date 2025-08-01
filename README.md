# Shos.StaffManager

A console-based staff management system built with .NET 8.0 and C#. This application provides an interactive command-line interface for managing staff members and departments with Japanese language support.

## Learning Value for Programming Students

This sample application serves as an excellent educational resource for developers learning modern C# and .NET development practices. The codebase demonstrates numerous important programming concepts and patterns:

### 🎯 **Core Programming Concepts**
- **Object-Oriented Programming**: Classes, inheritance, encapsulation, and polymorphism
- **Modern C# Features**: Records, nullable reference types, pattern matching, and collection expressions
- **SOLID Principles**: Single responsibility, dependency inversion, and interface segregation
- **Generic Programming**: Generic classes, methods, and type constraints

### 🏗️ **Software Architecture & Design Patterns**
- **Model-View-Controller (MVC)**: Clean separation between data, presentation, and business logic
- **Command Pattern**: Menu operations implemented as reusable command objects
- **Factory Pattern**: Command creation and management
- **Strategy Pattern**: Different validation rules and processing strategies
- **Template Method Pattern**: Base classes with customizable behavior

### 📊 **Data Management & Serialization**
- **JSON Serialization**: Custom serialization with `System.Text.Json`
- **Data Transfer Objects (DTOs)**: Separate serializable models for persistence
- **File I/O Operations**: UTF-8 encoding and error handling
- **Data Validation**: Input validation with custom rules and error messages

### 🔧 **Advanced C# Techniques**
- **Extension Methods**: String operations and type parsing utilities
- **Reflection**: Dynamic type parsing and method invocation
- **LINQ**: Functional query operations and data transformation
- **Tuple Deconstruction**: Modern return patterns with named tuples
- **Resource Management**: `IDisposable` pattern for console color management

### 🌐 **Internationalization & Localization**
- **Unicode Support**: Proper handling of Japanese characters (Zenkaku/Hankaku)
- **Character Width Calculation**: Display formatting for mixed-width text
- **Text Normalization**: Unicode normalization for consistent input handling

### 🎨 **User Interface Design**
- **Console UI Patterns**: Menu systems, dialogs, and tabular data display
- **Input Validation**: Real-time validation with user-friendly error messages
- **State Management**: Multi-step wizards and repeatable operations
- **User Experience**: Confirmation dialogs and navigation patterns

### 🧪 **Best Practices Demonstrated**
- **Error Handling**: Try-catch blocks with specific exception types
- **Code Documentation**: Comprehensive XML documentation comments
- **Naming Conventions**: Clear, descriptive method and variable names
- **Code Organization**: Logical namespace structure and file organization
- **Type Safety**: Nullable reference types and null checking

### 🎓 **Skill Development Areas**

**Beginner Level:**
- Basic C# syntax and object-oriented concepts
- File I/O and data persistence
- Console application development
- Error handling fundamentals

**Intermediate Level:**
- Design patterns implementation
- Generic programming
- LINQ and functional programming concepts
- Custom serialization strategies

**Advanced Level:**
- Reflection and metaprogramming
- Advanced type system features
- Architectural patterns
- Performance optimization techniques

### 📚 **Educational Exercises**

Students can enhance their learning by:
1. **Adding new features**: Implement edit/delete operations for staff and departments
2. **Extending validation**: Add more complex business rules and validation logic
3. **Database integration**: Replace JSON storage with Entity Framework Core
4. **Testing**: Write unit tests for business logic and validation
5. **Localization**: Add support for multiple languages
6. **Performance**: Implement caching and async operations
7. **UI enhancement**: Add color coding, progress bars, or menu shortcuts

This codebase provides a realistic, well-structured example that bridges the gap between simple tutorials and complex enterprise applications, making it an ideal learning resource for developers at various skill levels.

## Features

### Core Functionality
- **Staff Management**: Add, list, and search staff members
- **Department Management**: Add and list organizational departments  
- **Search Capabilities**: Find staff by name, employee number, or department
- **Data Persistence**: Automatic JSON file storage with UTF-8 encoding
- **Japanese Support**: Full support for Japanese characters with proper display width calculation

### User Interface
- Interactive console menu system
- Tabular data display using [Shos.Console](https://www.nuget.org/packages/Shos.Console/) library
- Input validation with error messages
- Confirmation dialogs for data entry operations
- Repeatable commands for bulk operations

## Technical Architecture

### Design Patterns
- **Model-View-Controller (MVC)**: Clean separation of concerns
- **Command Pattern**: Menu operations implemented as commands
- **Repository Pattern**: JSON-based data persistence layer

### Project Structure
```
Shos.StaffManager/
├── Models/
│   ├── Staff.cs              # Staff record with validation
│   ├── Department.cs         # Department record  
│   ├── Company.cs            # Main data container
│   └── SerializableStaff.cs  # JSON serialization support
├── Views/
│   └── View.cs               # Data display formatting
├── Controllers/
│   ├── Commands/             # Menu command implementations
│   └── CommandManager.cs     # Command orchestration
├── Common/
│   ├── Helpers/              # Utility classes
│   └── ControllersBase/      # Base UI components
└── Data/
    └── FC.StaffManager.json  # Sample data file
```

### Data Model

#### Staff Record
- **Number**: Employee ID (1-9999)
- **Name**: Full name in Japanese (1-30 characters)
- **Ruby**: Phonetic reading (katakana)
- **Department**: Associated department object

#### Department Record  
- **Code**: Department code (100-999)
- **Name**: Department name (1-30 characters)

### Data Validation
- Staff numbers must be unique within 1-9999 range
- Department codes must be unique within 100-999 range
- Name fields have 1-30 character length limits
- Department references are validated on staff creation

## Requirements

- **.NET 8.0** or later
- **Shos.Console 1.1.5** (automatically installed via NuGet)

## Installation & Setup

1. **Clone the repository**:
   ```bash
   git clone https://github.com/Fujiwo/Shos.StaffManager.git
   cd Shos.StaffManager
   ```

2. **Build the application**:
   ```bash
   dotnet build
   ```

3. **Run the application**:
   ```bash
   dotnet run --project Shos.StaffManager
   ```

## Usage

### Main Menu Options
The application presents a Japanese menu with the following options:

- **(s) 社員一覧** - List all staff members
- **(f) 社員検索** - Search for staff members  
- **(a) 社員追加** - Add new staff member
- **(d) 部署一覧** - List all departments
- **(e) 部署追加** - Add new department
- **(x) 終了** - Exit application

### Sample Operations

#### Adding a Department
1. Select **(e) 部署追加** from main menu
2. Enter department code (100-999)
3. Enter department name (1-30 characters)
4. Confirm the addition

#### Adding a Staff Member
1. Select **(a) 社員追加** from main menu  
2. Enter staff number (1-9999)
3. Enter staff name (1-30 characters)
4. Enter phonetic reading (ruby)
5. Enter existing department code
6. Confirm the addition

#### Searching Staff
1. Select **(f) 社員検索** from main menu
2. Enter search term (name, number, or department)
3. View filtered results
4. Repeat search or return to main menu

## Data Storage

### File Format
Data is stored in `FC.StaffManager.json` using UTF-8 encoding with pretty-printed JSON:

```json
{
  "SerializableDepartmentList": [
    {
      "Code": 114,
      "Name": "ウェブ開発部"
    }
  ],
  "SerializableStaffList": [
    {
      "Number": 2562,
      "Name": "森 誠", 
      "Ruby": "モリ マコト",
      "DepartmentCode": 114
    }
  ]
}
```

### Data Migration
The application automatically creates a new data file if none exists. Sample data is included in the `Data/` directory.

## Key Technologies

- **.NET 8.0**: Modern C# with latest language features
- **System.Text.Json**: High-performance JSON serialization
- **Shos.Console**: Enhanced console table formatting
- **Records**: Immutable data structures with validation
- **LINQ**: Functional query operations
- **UTF-8**: Full Unicode support for Japanese text

## Development Features

### Error Handling
- Comprehensive input validation
- Graceful error recovery
- User-friendly error messages in Japanese
- Type-safe parsing with fallback handling

### Extensibility
- Command pattern allows easy addition of new operations
- Generic type parsing supports multiple data types
- Modular architecture supports feature additions
- Separation of concerns enables unit testing

### Performance
- In-memory data operations
- Efficient JSON serialization
- Lazy loading of data
- Optimized string width calculations for Japanese text

## Contributing

This project uses standard .NET development practices:
- Follow existing code style and patterns
- Add unit tests for new features
- Ensure Japanese character support is maintained
- Update documentation for API changes

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.