namespace MfpDashboard.Models.CSV;

public sealed class MfpFoodCsvRowMap : ClassMap<MfpFoodCsvRow>
{
    public MfpFoodCsvRowMap()
    {
        AutoMap(System.Globalization.CultureInfo.InvariantCulture);
        Map(m => m.Fat).Name("Fat (g)", "Fat");
        Map(m => m.Cholesterol).Name("Cholesterol (mg)", "Cholesterol");
        Map(m => m.Sodium).Name("Sodium (mg)", "Sodium");
        Map(m => m.Carbohydrates).Name("Carbohydrates (g)", "Carbohydrates");
        Map(m => m.Fiber).Name("Fiber (g)", "Fiber");
        Map(m => m.Sugar).Name("Sugar (g)", "Sugar");
        Map(m => m.Protein).Name("Protein (g)", "Protein");
    }
}