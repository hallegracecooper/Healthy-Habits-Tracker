# Healthy Habits Tracker

A simple, intuitive web application built with .NET Blazor to help users track daily habits and build consistency in their routines. Perfect for college students and busy professionals who want to track habits like drinking water, exercising, reading, or any other daily routine.

## 🎯 Features

### Core Functionality

- **User Authentication** - Secure account creation and login
- **Habit Management** - Add, edit, and delete personal habits
- **Daily Tracking** - Mark habits as complete for each day
- **Progress Visualization** - View streaks and weekly summaries
- **Responsive Design** - Works seamlessly on desktop and mobile

### Advanced Features

- **Current Streak Calculation** - Tracks consecutive days of habit completion
- **Weekly Progress Reports** - Shows completion rates and progress bars
- **Motivational Feedback** - Encouraging messages based on progress
- **Real-time Updates** - Instant feedback when marking habits complete
- **Data Persistence** - All progress is saved and maintained

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022 or VS Code
- SQLite (included with .NET)

### Installation

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd Healthy-Habits-Tracker
   ```

2. **Navigate to the project directory**

   ```bash
   cd HealthyHabitsTracker
   ```

3. **Restore dependencies**

   ```bash
   dotnet restore
   ```

4. **Run database migrations**

   ```bash
   dotnet ef database update
   ```

5. **Start the application**

   ```bash
   dotnet run
   ```

6. **Open your browser**
   Navigate to `https://localhost:5001` (or the URL shown in the terminal)

## 📱 How to Use

### Creating an Account

1. Click "Register" on the homepage
2. Enter your email and password (minimum 6 characters)
3. Click "Create Account" to get started

### Adding Your First Habit

1. Click "New Habit" on the dashboard
2. Enter a clear, motivating title (e.g., "Drink 8 glasses of water")
3. Optionally add a description explaining why this habit matters
4. Click "Create Habit"

### Tracking Daily Progress

1. On the dashboard, you'll see all your habits
2. Click the circular button next to each habit to mark it complete
3. Completed habits show with a green border and "Completed" badge
4. Your streak counter shows consecutive days of completion

### Viewing Progress

- **Current Streak** - Shows consecutive days for each habit
- **Weekly Summary** - Displays total completions this week
- **Progress Bar** - Visual representation of weekly completion rate
- **Motivational Messages** - Encouraging feedback based on your progress

### Managing Habits

- **Edit** - Click the "Edit" button to modify habit details
- **Delete** - Click "Delete" to remove a habit (with confirmation)
- **Toggle Completion** - Click the circular button to mark complete/incomplete

## 🏗️ Technical Architecture

### Technology Stack

- **Frontend**: Blazor Server with Razor components
- **Backend**: ASP.NET Core with minimal APIs
- **Database**: SQLite with Entity Framework Core
- **Authentication**: ASP.NET Identity with cookie authentication
- **Styling**: Bootstrap 5 with custom CSS

### Project Structure

```
HealthyHabitsTracker/
├── Components/
│   ├── Layout/           # Navigation and layout components
│   └── Pages/            # Main application pages
├── Data/
│   └── AppDbContext.cs   # Database context and configuration
├── Models/               # Data models (Habit, AppUser, HabitCompletion)
├── Services/             # Business logic services
├── Migrations/           # Database migration files
└── wwwroot/              # Static files (CSS, JS, images)
```

### Key Components

- **Dashboard** - Main interface showing all habits and progress
- **HabitForm** - Create new habits with validation
- **HabitEdit** - Modify existing habits
- **Login/Register** - User authentication pages
- **NavMenu** - Navigation and logout functionality

## 🔧 Development

### Database Schema

- **AppUser** - User accounts with email and hashed passwords
- **Habit** - Individual habits with title, description, and completion status
- **HabitCompletion** - Daily completion records for streak calculation

### Key Services

- **HabitProgressService** - Calculates streaks and weekly summaries
- **ValidationService** - Input validation and duplicate checking
- **ErrorHandlingService** - User-friendly error messages

### Adding New Features

1. Create new Razor components in `Components/Pages/`
2. Add API endpoints in `Program.cs`
3. Update database models and run migrations
4. Add validation and error handling

## 🛡️ Security Features

- **Password Hashing** - Secure password storage using ASP.NET Identity
- **CSRF Protection** - Antiforgery tokens on all forms
- **User Isolation** - Users can only access their own data
- **Input Validation** - Comprehensive validation on all inputs
- **SQL Injection Prevention** - Parameterized queries with Entity Framework

## 🎨 Design Principles

### User Experience

- **Simplicity First** - Clean, uncluttered interface
- **Mobile Responsive** - Works on all device sizes
- **Accessibility** - ARIA labels, keyboard navigation, screen reader support
- **Visual Feedback** - Clear indicators for completed habits and progress

### Code Quality

- **Separation of Concerns** - Clear separation between UI, business logic, and data
- **Dependency Injection** - Proper service registration and usage
- **Async/Await** - Non-blocking database operations
- **Error Handling** - Comprehensive try/catch blocks with user feedback

## 📊 Performance Considerations

- **Database Optimization** - Indexed queries for fast habit retrieval
- **Caching** - Progress calculations cached during page load
- **Minimal Dependencies** - Lightweight application with minimal external packages
- **Responsive Loading** - Progressive loading with loading indicators

## 🐛 Troubleshooting

### Common Issues

**Database Connection Issues**

- Ensure SQLite is properly installed
- Run `dotnet ef database update` to apply migrations
- Check connection string in `appsettings.json`

**Authentication Problems**

- Clear browser cookies and try again
- Ensure user account exists in database
- Check password requirements (minimum 6 characters)

**Build Errors**

- Run `dotnet clean` then `dotnet build`
- Ensure .NET 9.0 SDK is installed
- Check for missing dependencies

### Getting Help

- Check the console output for detailed error messages
- Ensure all dependencies are restored with `dotnet restore`
- Verify database migrations are applied

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Built with .NET Blazor and ASP.NET Core
- UI components powered by Bootstrap 5
- Icons provided by Open Iconic
- Database management with Entity Framework Core

---

**Happy Habit Building! 🎯**
