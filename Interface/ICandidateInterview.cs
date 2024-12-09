using HiringProcess.Models;
using HiringProcess.Models.View_Model;
using System.Threading.Tasks;

namespace HiringProcess.Interface
{
    public interface ICandidateInterview
    {
        public Task<CandidateInterviewRoundScheduled> CandidateInterviewScheduledSave(CandidateInterviewRoundScheduled candidateInterviewRoundScheduled);
        public Task<List<CandidateInterviewVm>> CandidateInterviewScheduledGetList();
        public Task<bool> CandidateInterviewScheduledDelete(int Id);
        public Task<CandidateInterviewVm> GetCandidateInterviewScheduled(int Id);
        Task<List<Candidate>> GetCandidateList();
        Task<List<InterviewRound>> GetInterviewRoundList();



        //Re-Sceduled
        public Task<CandidateReScheduledInterview> CandidateInterviewReScheduledSave(CandidateReScheduledInterview candidateInterviewRoundReScheduled);
        public Task<List<CandidateReScheduledInterview>> CandidateInterviewReScheduledGetList();
        public Task<bool> CandidateInterviewReScheduledDelete(int Id);
        public Task<CandidateReScheduledInterview> GetCandidateInterviewReScheduled(int Id);


        //From CandidateId Interview Details

        public CandidateInterviewVm GetCandidateWithInterviews(int candidateId);
        public CandidateInterviewVm GetCandidateInterviewScheduledById(int Id);

        public Task<List<CandidateInterviewVm>> GetCandidateInterviewsScheduledByCandidateId(int CandidateId);

        public Task<bool> CandidateAllInterviewScheduledDelete(int candidateId);
        public Task<bool> CandidateAllInterviewReScheduledDelete(int candidateId);

        //Email

        Task<InterviewRound> GetInterviewRoundById(int roundId);
        public Task<CandidateInterviewRoundScheduled> IsCandidateAlreadyInterviewScheduled(int CandidateId, int RoundId);

        //All
        public Task<CandidateInterviewVm> CandidateInterviewScheduledAllSave(CandidateInterviewVm candidateInterviewVm);

        //Marks

        public Task<List<CandidateInterviewVm>> CandidateInterviewMarksGetList();
        Task<List<InterviewRound>> GetInterviewRoundWithQuizList();
        public Task<CandidateInterviewRoundResult> IsCandidateAlreadyInterviewMarks(int CandidateId, int RoundId);
        public Task<List<CandidateInterviewVm>> CandidateInterviewScheduledGetListById(int candidateId, int interviewRoundId);
        public Task<CandidateInterviewRoundResult> CandidateInterviewMarksSave(CandidateInterviewRoundResult candidateInterviewRoundResult);
        public Task<CandidateInterviewVm> GetCandidateInterviewMarks(int Id);
        public Task<bool> CandidateInterviewMarksDelete(int Id);
        public Task<bool> CandidateAllInterviewMarksDelete(int CandidateId);
        public CandidateInterviewVm GetCandidateWithInterviewsMarks(int candidateId);
        public CandidateInterviewVm GetCandidateInterviewMarksById(int Id);


        public CandidateInterviewVm GetInterviewMarksByCandidateId(int candidateId);

    }
}
