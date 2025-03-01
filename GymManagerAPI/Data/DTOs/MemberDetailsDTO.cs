namespace GymManagerAPI.Data.DTOs
{
    public class MemberDetailsDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string GenderName { get; set; }

        public string Ci { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime? PlanExpirationDate { get; set; } = null;

        public bool HasPlanActive
        {
            get
            {
                if (PlanExpirationDate.HasValue)
                {
                    if(PlanExpirationDate.Value >= DateTime.Now)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
