using HiringProcess.Models;
using HiringProcess.Models.View_Model;

namespace HiringProcess.Interface
{
    public interface IQuestionAnswer
    {
        Task<List<QuizQuestionAnswer>> QuizQuestionAnswerGetList();
        Task<int> QuizQuestionAnswerCount();
        public Task<QuizQuestionsAnswerEntryVM> GetQuizQuestionAnswer(int QuizQuestionAnswerId);
        public Task<bool> DeleteQuizQuestionAnswer(int QuizQuestionAnswerId);

        public Task<PracticalExamQuestion> GetPositionQuestionById(int Id);
        public Task<List<PracticalExamQuestion>> PracticalExamQuestionGetList();
        public Task<bool> DeletePracticalExamQuestion(int Id);

    }
}
