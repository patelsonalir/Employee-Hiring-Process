using HiringProcess.Models;
using Microsoft.EntityFrameworkCore;

namespace HiringProcess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Admin> Admin { get; set; }
        public DbSet<Position> Position { get; set; }
        public DbSet<Candidate> Candidate { get; set; }
        public DbSet<Quiz> Quiz { get; set; }
        public DbSet<QuizQuestionAnswer> QuizQuestionAnswer { get; set; }
        public DbSet<CandidateQuizResult> CandidateQuizResult { get; set; }
        public DbSet<CandidateInterviewRoundScheduled> CandidateInterviewRoundScheduled { get; set; }
        public DbSet<CandidateInterviewRoundResult> CandidateInterviewRoundResult { get; set; }
        public DbSet<CandidateReScheduledInterview> CandidateReScheduledInterview { get; set; }
        public DbSet<PracticalExamQuestion> PracticalExamQuestion { get; set; }
        public DbSet<InterviewRound> InterviewRound { get; set; }      

    }
}
