# Shos.StaffManager

[日本語版 README はこちら / Japanese version](README.jp.md)

A console-based staff management system built with .NET 10.0 and C#. This application provides an interactive command-line interface for managing staff members and departments with Japanese language support, an in-app AI chat assistant built with the Microsoft Agent Framework, and a standalone MCP (Model Context Protocol) server for external AI assistant integration.

## Purpose and Intended Use

Shos.StaffManager is a sample application whose goal is to show how a small, realistic line-of-business tool can be written in modern C# and then opened up to AI assistants.

It is intended to be used for:

- **Managing a small staff roster**: keeping a list of departments and staff members (number, name, phonetic reading, department) in a single JSON file, edited through an interactive Japanese console UI
- **Learning modern .NET**: studying records, generics, LINQ, `System.Text.Json` serialization, the command pattern, and console UI construction in a codebase that is larger than a tutorial but still small enough to read end to end
- **Experimenting with AI integration**: talking to the roster through the built-in AI chat (Microsoft Agent Framework + Ollama or Azure OpenAI), and exposing the same operations to external AI assistants such as Claude Desktop or VS Code through the bundled MCP server

It is **not** intended as a production HR system: there is no database, no authentication, and no concurrency control beyond last-writer-wins on the JSON file.

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
- **AI Integration**: Model Context Protocol (MCP) server development
- **Tool Design**: Creating AI-accessible tools and APIs

### 📚 **Educational Exercises**

Students can enhance their learning by:
1. **Adding new features**: Implement edit/delete operations for staff and departments
2. **Extending validation**: Add more complex business rules and validation logic
3. **Database integration**: Replace JSON storage with Entity Framework Core
4. **Testing**: Write unit tests for business logic and validation
5. **Localization**: Add support for multiple languages
6. **Performance**: Implement caching and async operations
7. **UI enhancement**: Add color coding, progress bars, or menu shortcuts
8. **MCP expansion**: Add new MCP tools for reporting and analytics
9. **AI integration**: Experiment with different AI assistants using the MCP server
10. **Protocol development**: Create custom MCP tools for specialized workflows

This codebase provides a realistic, well-structured example that bridges the gap between simple tutorials and complex enterprise applications, making it an ideal learning resource for developers at various skill levels.

## Features

### Core Functionality
- **Staff Management**: Add, list, search, and reassign the department of staff members
- **Department Management**: Add, list, search, and rename organizational departments
- **Search Capabilities**: Find staff by name, employee number, or department; find departments by name or code
- **Data Persistence**: Automatic JSON file storage with UTF-8 encoding
- **Japanese Support**: Full support for Japanese characters with proper display width calculation
- **MCP Integration**: Model Context Protocol server for AI assistant integration

### User Interface
- Interactive console menu system
- Tabular data display using [Shos.Console](https://www.nuget.org/packages/Shos.Console/) library
- Input validation with error messages
- Confirmation dialogs for data entry operations
- Repeatable commands for bulk operations

### AI Chat (In-App)
- **Embedded AI Assistant**: The **(c) AIチャット** menu command opens a chat with an AI agent built on the [Microsoft Agent Framework](https://www.nuget.org/packages/Microsoft.Agents.AI/), running directly inside the console app
- **Pluggable Chat Backend**: Uses a local [Ollama](https://ollama.com/) server (`http://localhost:11434`, model `minimax-m3:cloud`) by default, or Azure OpenAI when the `AZURE_OPENAI` compilation symbol is enabled
- **Tool Calling**: The agent can call the staff/department management tools (`Toolbox`) as well as an external Brave Search MCP tool launched via `npx @modelcontextprotocol/server-brave-search`

### AI Assistant Integration (MCP Server)
- **MCP Server**: The separate `Shos.StaffManager.MCPServer` project exposes staff management functionality through the Model Context Protocol for external AI assistants (Claude Desktop, VS Code, etc.)
- **Tool-based Access**: AI assistants can directly manage staff and departments
- **Real-time Operations**: Add, remove, search, rename, and reassign operations available via MCP tools

## Technical Architecture

### Design Patterns
- **Model-View-Controller (MVC)**: Clean separation of concerns
- **Command Pattern**: Menu operations implemented as commands
- **Repository Pattern**: JSON-based data persistence layer

### Class Diagrams

Up-to-date Mermaid class diagrams for the whole solution are maintained separately in [Documents/ClassDiagrams.md](Documents/ClassDiagrams.md):

- **Namespace diagram**: dependency relationships between all namespaces across the 3 projects (`Shos.StaffManager`, `Shos.StaffManager.Models`, `Shos.StaffManager.MCPServer`)
- **Class diagram**: every class/record in the solution (inheritance, composition, aggregation, and dependency relationships), grouped by namespace

### Project Structure
```
Shos.StaffManager/
├── Shos.StaffManager/                  # Main console application
│   ├── Program.cs                        # Entry point (Load → menu loop → Save)
│   ├── Application/
│   │   ├── View.cs                       # Data display formatting (Views namespace)
│   │   └── Controllers.cs                # Menu commands + CommandManager (Controllers namespace)
│   ├── AI/
│   │   ├── AIAgent.cs                    # ChatAgent / MyChatAgent (Microsoft Agent Framework)
│   │   └── ToolBox.cs                    # Function-calling tools for the in-app AI chat
│   ├── Common/
│   │   ├── Helpers.cs                    # UserInterface, TypeParser, extension methods
│   │   └── ControllerBase.cs             # Window, Command<TModel>, Menu<TModel> base types
│   └── Data/
│       └── FC.StaffManager.json          # Sample data file
├── Shos.StaffManager.Models/            # Shared data models library
│   └── Models.cs                         # Department, Staff, DepartmentList, StaffList, Company
└── Shos.StaffManager.MCPServer/         # MCP server for external AI assistant integration
    └── Program.cs                        # Host setup + StaffManagerTools (MCP tool definitions)
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

## Shos.StaffManager.MCPServer

### What is MCP (Model Context Protocol)?

**Model Context Protocol (MCP)** is an open standard that enables secure connections between AI assistants and external data sources and tools. MCP allows AI assistants like Claude, ChatGPT, and others to access and interact with external systems through a standardized protocol, expanding their capabilities beyond their training data.

Key benefits of MCP:
- **Secure Integration**: Controlled access to external tools and data
- **Real-time Data**: Access to live, up-to-date information
- **Tool Execution**: AI assistants can perform actions on external systems
- **Standardized Protocol**: Consistent interface across different AI platforms

### MCP Server Component

The `Shos.StaffManager.MCPServer` project provides an MCP server that exposes the staff management system's functionality as tools that AI assistants can use. This allows AI assistants to directly interact with the staff management system without requiring manual console operations.

#### Available MCP Tools

The MCP server provides the following tools for AI assistants:

**Department Management:**
- `GetAllDepartments()` - Retrieve all departments
- `SearchDepartments(keyword)` - Search departments by keyword
- `AddNewDepartment(newDepartment)` - Add a new department
- `RemoveDepartmentByCode(departmentCode)` - Remove a department by code
- `ChangeDepartmentNameByCode(departmentCode, newName)` - Rename a department by code

**Staff Management:**
- `GetAllStaffs()` - Retrieve all staff members
- `SearchStaffs(keyword)` - Search staff by keyword
- `AddNewStaff(newStaff)` - Add a new staff member
- `RemoveStaffbyNumber(number)` - Remove a staff member by number
- `ChangeStaffsDepartmentByNumber(number, newDepartmentCode)` - Reassign a staff member's department by staff number

#### Setting Up the MCP Server

1. **Build the MCP Server**:
   ```bash
   dotnet build Shos.StaffManager.MCPServer
   ```

2. **Run with MCP Inspector** (for testing):
   ```bash
   npx @modelcontextprotocol/inspector dotnet run --project ./Shos.StaffManager.MCPServer/Shos.StaffManager.MCPServer.csproj
   ```

3. **Configure with Claude Desktop**:
   
   Edit your `claude_desktop_config.json` file:
   ```json
   {
     "mcpServers": {
       "StaffManagerTools": {
         "command": "[path-to-executable]\\Shos.StaffManager.MCPServer.exe"
       }
     }
   }
   ```

4. **Configure with Visual Studio Code**:
   
   Add to your VS Code MCP settings:
   ```json
   {
     "servers": {
       "MCPServer.Console": {
         "type": "stdio",
         "command": "dotnet",
         "args": [
           "run",
           "--project",
           "C:\\[project-folder]\\Shos.StaffManager.MCPServer.csproj"
         ]
       }
     }
   }
   ```

#### Use Cases for AI Integration

With the MCP server running, AI assistants can:
- **Answer questions** about staff and departments using live data
- **Generate reports** by querying and analyzing staff information
- **Perform bulk operations** like adding multiple staff members
- **Validate data integrity** by checking for duplicates or inconsistencies
- **Provide interactive assistance** for staff management tasks

This integration transforms the console-based staff management system into a powerful backend service that AI assistants can leverage for intelligent staff management operations.

## Requirements

### Console Application
- **.NET 10.0 SDK** or later (Windows, macOS, or Linux; Visual Studio 2026 or later is optional)
- **Shos.Console 1.1.5**, **Shos.UndoRedoList 1.0.5** (via `Shos.StaffManager.Models`)
- **Microsoft.Agents.AI 1.14.0** / **Microsoft.Agents.AI.OpenAI 1.14.0** - Microsoft Agent Framework for the in-app AI chat
- **OllamaSharp 5.4.27** - local LLM chat client (default backend)
- **Azure.AI.OpenAI 2.1.0** - optional Azure OpenAI chat client (enable via the `AZURE_OPENAI` compilation symbol)
- **ModelContextProtocol 1.4.1** - MCP client used by the in-app AI chat to call an external Brave Search tool

(all packages above are automatically installed via NuGet)

Optional, only for the **(c) AIチャット** command:

- A running [Ollama](https://ollama.com/) server on `http://localhost:11434` with the `minimax-m3:cloud` model, **or** the `AZURE_OPENAI_ENDPOINT` / `AZURE_OPENAI_API_KEY` environment variables when built with the `AZURE_OPENAI` compilation symbol
- [Node.js](https://nodejs.org/) (for `npx`) and a `BRAVE_API_KEY` environment variable for the external Brave Search MCP tool

### MCP Server
- **.NET 10.0 SDK** or later
- **ModelContextProtocol 1.4.1** (automatically installed via NuGet)
- **Microsoft.Extensions.Hosting 10.0.10** (automatically installed via NuGet)

## Installation & Build

### Console Application

1. **Clone the repository**:
   ```bash
   git clone https://github.com/Fujiwo/Shos.StaffManager.git
   cd Shos.StaffManager
   ```

2. **Build the application**:

   With the .NET CLI:
   ```bash
   dotnet build
   ```

   With Visual Studio:
   1. Open `Shos.StaffManager.sln`
   2. Set `Shos.StaffManager` as the startup project
   3. Build the solution (**Build** > **Build Solution**, or `Ctrl+Shift+B`)

3. **Run the console application**:
   ```bash
   dotnet run --project Shos.StaffManager
   ```
   In Visual Studio, press `F5` (debug) or `Ctrl+F5` (without debugging).

### MCP Server Setup

1. **Build the MCP server**:
   ```bash
   dotnet build Shos.StaffManager.MCPServer
   ```

2. **Test with MCP Inspector**:
   ```bash
   npx @modelcontextprotocol/inspector dotnet run --project Shos.StaffManager.MCPServer
   ```

3. **Configure your AI assistant** (see MCP Server section above for detailed configuration)

## Usage

### Main Menu Options
The application presents a Japanese menu with the following options:

- **(s) 社員一覧** - List all staff members
- **(f) 社員検索** - Search for staff members
- **(a) 社員追加** - Add a new staff member
- **(d) 部署一覧** - List all departments
- **(e) 部署追加** - Add a new department
- **(c) AIチャット** - Chat with the in-app AI assistant
- **(x) 終了** - Exit the application

### Sample Operations

#### Adding a Department
1. Select **(e) 部署追加** from the main menu
2. Enter a department code (100-999)
3. Enter a department name (1-30 characters)
4. Confirm the addition

#### Adding a Staff Member
1. Select **(a) 社員追加** from the main menu  
2. Enter a staff number (1-9999)
3. Enter a staff name (1-30 characters)
4. Enter the phonetic reading (ruby)
5. Enter an existing department code
6. Confirm the addition

#### Searching for Staff
1. Select **(f) 社員検索** from the main menu
2. Enter a search term (name, number, or department)
3. View the filtered results
4. Repeat the search or return to the main menu

## Data Storage

### File Location
Both the console application and the MCP server read and write the same hard-coded file path, `C:\work\FC.StaffManager.json` (defined as `dataFilePath` in `Shos.StaffManager/Program.cs` and `Shos.StaffManager.MCPServer/Program.cs`). Change that constant to use a different location.

The console application loads the file at startup and saves it on exit, while the MCP server saves after every mutating tool call, so running both against the same file at the same time is last-writer-wins.

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

### Getting Started with Data
If the file does not exist, the application starts with an empty company and creates the file on the next save. Sample data is included in `Shos.StaffManager/Data/FC.StaffManager.json`; copy it to `C:\work\FC.StaffManager.json` to start with populated departments and staff.

## Key Technologies

### Core Technologies
- **.NET 10.0**: Modern C# with latest language features
- **System.Text.Json**: High-performance JSON serialization
- **Shos.Console**: Enhanced console table formatting
- **Shos.UndoRedoList**: Undo/redo-capable collections used internally by `DepartmentList`/`StaffList`
- **Records**: Immutable data structures with validation
- **LINQ**: Functional query operations
- **UTF-8**: Full Unicode support for Japanese text

### AI Chat Integration (In-App)
- **Microsoft Agent Framework** (`Microsoft.Agents.AI`): Agent/session abstraction for the "AIチャット" menu command
- **OllamaSharp**: Default local LLM chat client (`http://localhost:11434`)
- **Azure.AI.OpenAI**: Optional Azure OpenAI chat client (behind the `AZURE_OPENAI` compilation symbol)
- **ModelContextProtocol (client)**: Lets the in-app agent call an external Brave Search MCP tool alongside its own `Toolbox` functions

### MCP Integration (Server)
- **ModelContextProtocol**: Standard protocol for AI assistant tool integration
- **Microsoft.Extensions.Hosting**: .NET hosting model for the MCP server
- **Stdio Transport**: Standard input/output communication with AI assistants

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
- In-memory data operations for fast access
- Efficient JSON serialization and deserialization
- Lazy loading of data when needed
- Optimized string width calculations for Japanese text display

## Contributing

This project follows standard .NET development practices:
- Follow the existing code style and patterns
- Add unit tests for any new features
- Ensure Japanese character support is maintained
- Update documentation for any API changes

## License

This project is licensed under the MIT License, a permissive license that allows free use, modification, and redistribution (including commercial use) as long as the copyright notice and license text are included. See the [LICENSE.txt](LICENSE.txt) file for details.