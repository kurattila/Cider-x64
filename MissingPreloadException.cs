using System;
using System.Runtime.Serialization;

namespace Cider_x64
{
    [Serializable]
    public class MissingPreloadException : Exception
    {
        public MissingPreloadException(string message, Exception innerException)
            : base(message, innerException)
        { }

        // needed to make MissingPreloadException serializable between AppDomains
        protected MissingPreloadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public static readonly string TitleTextOfAdvice = "Whoops, you can fix this issue...";
        public static readonly string IntroPartOfAdvice = "...by appending your \"Preloaded Assemblies\" list:";
        public string GetAdviceForUser()
        {
            string message = InnermostExceptionExtractor.GetInnermostMessage(this);

            return string.Format("{0}{1}{2}{3}", MissingPreloadException.IntroPartOfAdvice, Environment.NewLine, Environment.NewLine, message);
        }
    }

    public static class InnermostExceptionExtractor
    {
        public static string GetInnermostMessage(Exception e)
        {
            Exception innermostException = e;
            while (innermostException.InnerException != null)
                innermostException = innermostException.InnerException;

            return innermostException.Message;
        }
    }
}
