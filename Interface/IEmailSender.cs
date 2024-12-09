namespace HiringProcess.Interface
{
    public interface IEmailSender
    {
        Task SendInterviewEmail(string Subject, string candidateEmail, string Body);
    }
}
