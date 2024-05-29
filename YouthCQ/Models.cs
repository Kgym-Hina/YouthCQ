namespace YouthCQ;

public class Models
{
    public class Course
    {
        public string id { get; set; }
        public string yearQuarterId { get; set; }
        public string periodsId { get; set; }
        public string courseTitle { get; set; }
        public string courseUrl { get; set; }
        public string RequireCompletion { get; set; }
        public string EnrollmentBaseVersionId { get; set; }
        public string ReleaseTime { get; set; }
        public int IsTop { get; set; }
        public int IsRelease { get; set; }
        public int IsNewTag { get; set; }
        public string learnCondition { get; set; }
    }
}