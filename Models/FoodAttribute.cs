namespace Models;

public enum FoodAttribute {
    Name,
    Carbs,
    Fats,
    Proteins,
    Fibres,
    Sugar,
    Starch,
    Saturated,
    Monounsaturated,
    Polyunsaturated,
    Trans,
    Cholesterol,
    Omega3,
    Omega6,
    Sodium,
    Potassium,
    Magnesium,
    Calcium,
    Zinc,
    Iron,
    Alcohol,
    Calories,
    FatsPercentage,
    ProteinsPercentage,
    CarbsPercentage,
    DetailsPercentage
}

public static class FoodAttributes {

    // not the prettiest
    public static string Nullable =
        string.Join(", ", new[] {
            FoodAttribute.Fibres,
            FoodAttribute.Sugar,
            FoodAttribute.Starch,
            FoodAttribute.Saturated,
            FoodAttribute.Monounsaturated,
            FoodAttribute.Polyunsaturated,
            FoodAttribute.Trans,
            FoodAttribute.Cholesterol,
            FoodAttribute.Omega3,
            FoodAttribute.Omega6,
            FoodAttribute.Sodium,
            FoodAttribute.Potassium,
            FoodAttribute.Magnesium,
            FoodAttribute.Calcium,
            FoodAttribute.Zinc,
            FoodAttribute.Iron,
            FoodAttribute.Alcohol 
        }.Select(attribute => attribute.ToString().ToLower()));

}