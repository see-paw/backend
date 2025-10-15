namespace Domain.Enums
{
    /// <summary>
    /// Represents the breed of an animal.
    /// </summary>
    /// <remarks>
    /// Includes a selection of the most common dog and cat breeds, 
    /// as well as generic options such as <see cref="Unknown"/>, <see cref="MixedBreed"/>, and <see cref="Mongrel"/>.
    /// This list can be expanded or localized according to shelter or regional needs.
    /// </remarks>
    public enum Breed
    {
        /// <summary>
        /// Breed is unknown or not specified.
        /// </summary>
        Unknown,

        /// <summary>
        /// The animal is of mixed or indeterminate breed.
        /// </summary>
        MixedBreed,

        /// <summary>
        /// A mongrel or mixed-breed dog (commonly referred to as "rafeiro").
        /// </summary>
        Mongrel,

        // Common dog breeds
        LabradorRetriever,
        GermanShepherd,
        GoldenRetriever,
        FrenchBulldog,
        Bulldog,
        Labrador,
        Poodle,
        Beagle,
        Rottweiler,
        YorkshireTerrier,
        Boxer,
        Dachshund,
        SiberianHusky,
        Chihuahua,
        Doberman,
        DobermanPinscher,
        ShihTzu,
        BorderCollie,
        Pug,
        CockerSpaniel,
        AustralianShepherd,
        Pomeranian,
        GreatDane,
        Maltese,
        BerneseMountainDog,
        CavalierKingCharlesSpaniel,
        ShibaInu,
        BostonTerrier,

        // Common cat breeds
        Persian,
        MaineCoon,
        Siamese,
        Ragdoll,
        BritishShorthair,
        Bengal,
        Sphynx,
        ScottishFold,
        Abyssinian,
        RussianBlue,
        NorwegianForestCat,
        Birman,
        OrientalShorthair,
        DevonRex,
        AmericanShorthair,
        ExoticShorthair,
        Burmese,
        TurkishAngora,
        DomesticShorthair,
        DomesticLonghair
    }
}
