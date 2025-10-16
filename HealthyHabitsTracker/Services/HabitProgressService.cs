using HealthyHabitsTracker.Data;
using HealthyHabitsTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthyHabitsTracker.Services
{
    /// <summary>
    /// Service for calculating habit progress metrics including streaks and weekly summaries
    /// Handles all business logic related to habit completion tracking and progress calculations
    /// </summary>
    public class HabitProgressService
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes the progress service with database context
        /// </summary>
        /// <param name="context">Entity Framework database context for data access</param>
        public HabitProgressService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calculates the current streak for a specific habit
        /// </summary>
        public async Task<int> GetCurrentStreakAsync(int habitId, string userId)
        {
            var today = DateTime.UtcNow.Date;
            var completions = await _context.HabitCompletions
                .Where(hc => hc.HabitId == habitId && hc.UserId == userId)
                .OrderByDescending(hc => hc.CompletionDate)
                .ToListAsync();

            if (!completions.Any())
                return 0;

            int streak = 0;
            var currentDate = today;

            foreach (var completion in completions)
            {
                var completionDate = completion.CompletionDate.Date;

                // If this completion is for today or yesterday, continue the streak
                if (completionDate == currentDate || completionDate == currentDate.AddDays(-1))
                {
                    if (completionDate == currentDate)
                    {
                        streak++;
                        currentDate = currentDate.AddDays(-1);
                    }
                    else if (completionDate == currentDate.AddDays(-1))
                    {
                        streak++;
                        currentDate = currentDate.AddDays(-1);
                    }
                }
                else
                {
                    // Gap found, streak is broken
                    break;
                }
            }

            return streak;
        }

        /// <summary>
        /// Calculates the weekly completion count for a user
        /// </summary>
        public async Task<int> GetWeeklyCompletionCountAsync(string userId)
        {
            var startOfWeek = GetStartOfWeek(DateTime.UtcNow);
            var endOfWeek = startOfWeek.AddDays(7);

            return await _context.HabitCompletions
                .Where(hc => hc.UserId == userId &&
                           hc.CompletionDate >= startOfWeek &&
                           hc.CompletionDate < endOfWeek)
                .CountAsync();
        }

        /// <summary>
        /// Gets the total number of habits for a user
        /// </summary>
        public async Task<int> GetTotalHabitsCountAsync(string userId)
        {
            return await _context.Habits
                .Where(h => h.UserId == userId)
                .CountAsync();
        }

        /// <summary>
        /// Gets the completion rate for this week
        /// </summary>
        public async Task<double> GetWeeklyCompletionRateAsync(string userId)
        {
            var totalHabits = await GetTotalHabitsCountAsync(userId);
            if (totalHabits == 0) return 0;

            var weeklyCompletions = await GetWeeklyCompletionCountAsync(userId);
            var daysInWeek = 7;
            var maxPossibleCompletions = totalHabits * daysInWeek;

            return maxPossibleCompletions > 0 ? (double)weeklyCompletions / maxPossibleCompletions * 100 : 0;
        }

        /// <summary>
        /// Gets the best streak for a specific habit
        /// </summary>
        public async Task<int> GetBestStreakAsync(int habitId, string userId)
        {
            var completions = await _context.HabitCompletions
                .Where(hc => hc.HabitId == habitId && hc.UserId == userId)
                .OrderBy(hc => hc.CompletionDate)
                .ToListAsync();

            if (!completions.Any())
                return 0;

            int bestStreak = 0;
            int currentStreak = 0;
            DateTime? lastDate = null;

            foreach (var completion in completions)
            {
                var completionDate = completion.CompletionDate.Date;

                if (lastDate == null || completionDate == lastDate.Value.AddDays(1))
                {
                    currentStreak++;
                }
                else
                {
                    bestStreak = Math.Max(bestStreak, currentStreak);
                    currentStreak = 1;
                }

                lastDate = completionDate;
            }

            return Math.Max(bestStreak, currentStreak);
        }

        /// <summary>
        /// Records a habit completion for today
        /// </summary>
        public async Task RecordCompletionAsync(int habitId, string userId)
        {
            var today = DateTime.UtcNow.Date;

            // Check if already completed today
            var existingCompletion = await _context.HabitCompletions
                .FirstOrDefaultAsync(hc => hc.HabitId == habitId &&
                                         hc.UserId == userId &&
                                         hc.CompletionDate.Date == today);

            if (existingCompletion == null)
            {
                var completion = new HabitCompletion
                {
                    HabitId = habitId,
                    UserId = userId,
                    CompletionDate = today
                };

                _context.HabitCompletions.Add(completion);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Removes a habit completion for today
        /// </summary>
        public async Task RemoveCompletionAsync(int habitId, string userId)
        {
            var today = DateTime.UtcNow.Date;

            var completion = await _context.HabitCompletions
                .FirstOrDefaultAsync(hc => hc.HabitId == habitId &&
                                         hc.UserId == userId &&
                                         hc.CompletionDate.Date == today);

            if (completion != null)
            {
                _context.HabitCompletions.Remove(completion);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Gets completion statistics for the dashboard
        /// </summary>
        public async Task<HabitProgressStats> GetProgressStatsAsync(string userId)
        {
            var totalHabits = await GetTotalHabitsCountAsync(userId);
            var weeklyCompletions = await GetWeeklyCompletionCountAsync(userId);
            var weeklyRate = await GetWeeklyCompletionRateAsync(userId);

            return new HabitProgressStats
            {
                TotalHabits = totalHabits,
                WeeklyCompletions = weeklyCompletions,
                WeeklyCompletionRate = weeklyRate
            };
        }

        private static DateTime GetStartOfWeek(DateTime date)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            return date.AddDays(-dayOfWeek).Date;
        }
    }

    public class HabitProgressStats
    {
        public int TotalHabits { get; set; }
        public int WeeklyCompletions { get; set; }
        public double WeeklyCompletionRate { get; set; }
    }
}
