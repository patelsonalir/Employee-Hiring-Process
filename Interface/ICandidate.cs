using HiringProcess.Models;
using System.Data;

namespace HiringProcess.Interface
{
    public interface ICandidate
    {
        public Task<Candidate> CandidateSave(Candidate CandidateMst);
        public Task<List<Candidate>> CandidateGetList();
        public Task<bool> CandidateDelete(int CandidateId);
        public Task<Candidate> GetCandidate(int CandidateId);
        Task<int> CandidateCount();
        Task<List<Position>> GetPositionList();
        public Task<bool> CandidateInterviewRoundChange(CandidateInterviewRoundResult candidateInterviewRoundResult);
        //Task<List<InterviewRound>> GetInterviewRoundList();
        //public Task<CandidateInterviewRoundScheduled> CandidateInterviewRoundScheduledSave(CandidateInterviewRoundScheduled interviewRoundScheduled);
        //public Task<CandidateReScheduledInterview> CandidateReScheduledInterviewdSave(CandidateReScheduledInterview candidateReScheduledInterview);

    }
}
