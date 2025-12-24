namespace Zumra.Utilites;

public static class Dictionary
{
    public static Dictionary<string, List<string>> CountriesWithCities =
        new Dictionary<string, List<string>>()
        {
            { "Egypt", new List<string>
                {
                    "Cairo",
                    "Alexandria",
                    "Giza",
                    "Aswan",
                    "Asyut",
                    "Beheira",
                    "Beni Suef",
                    "Dakahlia",
                    "Damietta",
                    "Faiyum",
                    "Gharbia",
                    "Ismailia",
                    "Kafr El Sheikh",
                    "Luxor",
                    "Matrouh",
                    "Minya",
                    "Monufia",
                    "New Valley",
                    "North Sinai",
                    "Port Said",
                    "Qalyubia",
                    "Qena",
                    "Red Sea",
                    "Sharqia",
                    "Sohag",
                    "South Sinai",
                    "Suez"
                }
            },
            { "Saudi Arabia", new List<string>
                {
                    "Riyadh",
                    "Makkah",
                    "Medina",
                    "Eastern Province",
                    "Qassim",
                    "Ha'il",
                    "Tabuk",
                    "Northern Borders",
                    "Jazan",
                    "Najran",
                    "Asir",
                    "Al Bahah",
                    "Al Jawf"
                }
            }
        };
}