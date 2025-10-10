namespace WebApplication2.TestClasses
{
    public class License
    {

        public int ID { get; set; }

        public string Name { get; set; }    

        public string Type { get; set; }

        private DateTime issueDate;
        public DateTime IssueDate {
            get 
            { 
                return issueDate; 
            } 
            set 
            {
                issueDate = value;
                ExpirationDate = issueDate.AddYears(10);
                
            } 
        } 

        public DateTime ExpirationDate { get; set; }
    }
}
