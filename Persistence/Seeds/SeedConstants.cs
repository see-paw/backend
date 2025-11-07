namespace Persistence.Seeds;

/// <summary>
/// Contains all shared constants used across different seed classes.
/// Centralizes IDs, URLs, and other static data to ensure consistency.
/// </summary>
internal static class SeedConstants
{
    // ======== BREED IDS ========
    public const string Breed1Id = "1a1a1111-1111-1111-1111-111111111111";
    public const string Breed2Id = "2b2b2222-2222-2222-2222-222222222222";
    public const string Breed3Id = "3c3c3333-3333-3333-3333-333333333333";
    
    // Ownership request test breeds
    public const string OwnershipBreed1Id = "breed-1";
    public const string OwnershipBreed2Id = "breed-2";
    public const string OwnershipBreed3Id = "breed-3";
    public const string OwnershipBreed4Id = "breed-4";
    
    // Fostering test breed
    public const string FosteringBreedId = "d4e5f6a7-b8c9-4d8e-1f2a-3b4c5d6e7f8a";
    
    // Cancel fostering test breed
    public const string CancelBreedId = "f4d5e6f7-a8b9-4c0d-1e2f-3a4b5c6d7e8f";

    // ======== SHELTER IDS ========
    public const string Shelter1Id = "11111111-1111-1111-1111-111111111111";
    public const string Shelter2Id = "22222222-2222-2222-2222-222222222222";
    public const string Shelter3Id = "33333333-3333-3333-3333-333333333333";
    
    // Ownership request test shelters
    public const string OwnershipShelter1Id = "shelter-1";
    public const string OwnershipShelter2Id = "shelter-2";
    
    // Fostering test shelter
    public const string FosteringShelterId = "c3d4e5f6-a7b8-4c7d-0e1f-2a3b4c5d6e7f";
    
    // Cancel fostering test shelter
    public const string CancelShelterId = "e3c4d5e6-f7a8-4b9c-0d1e-2f3a4b5c6d7e";

    // ======== ANIMAL IDS ========
    public const string Animal1Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd1b";
    public const string Animal2Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd2b";
    public const string Animal3Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd3b";
    public const string Animal4Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd4b";
    public const string Animal5Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd5b";
    public const string Animal6Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd6b";
    public const string Animal7Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd7b";
    public const string Animal8Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd8b";
    public const string Animal9Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd9b";
    public const string Animal10Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd0c";
    public const string Animal11Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd1c";
    public const string Animal12Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd1d";
    public const string Animal13Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd2d";
    public const string Animal14Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd3d";
    public const string Animal15Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd5d";
    
    // Eligibility test animals
    public const string AnimalAvailableId = "available-animal-id-123";
    public const string AnimalWithOwnerId = "animal-with-owner-id";
    public const string AnimalInactiveId = "inactive-animal-id";
    public const string AnimalPartiallyFosteredId = "partially-fostered-animal-id";
    public const string AnimalTotallyFosteredId = "totally-fostered-animal-id";
    
    // Ownership request test animals
    public const string OwnershipAnimal1Id = "animal-1";
    public const string OwnershipAnimal2Id = "animal-2";
    public const string OwnershipAnimal3Id = "animal-3";
    public const string OwnershipAnimal4Id = "animal-4";
    public const string OwnershipAnimal5Id = "animal-5";
    public const string OwnershipAnimal6Id = "animal-6";
    public const string OwnershipAnimal7Id = "animal-7";
    public const string OwnershipAnimal8Id = "animal-8";
    
    // Fostering test animals
    public const string AnimalF1Id = "e5f6a7b8-c9d0-4e9f-2a3b-4c5d6e7f8a9b";
    public const string AnimalF2Id = "f6a7b8c9-d0e1-4f0a-3b4c-5d6e7f8a9b0c";
    public const string AnimalF3Id = "animal-foster-003";
    public const string AnimalFInactiveId = "a7b8c9d0-e1f2-4a1b-4c5d-6e7f8a9b0c1d";
    public const string AnimalFAvailableId = "b8c9d0e1-f2a3-4b2c-5d6e-7f8a9b0c1d2e";
    public const string AnimalWithSlotId = "c9d0e1f2-a3b4-4c3d-6e7f-8a9b0c1d2e3f";
    public const string AnimalWithActivityId = "d0e1f2a3-b4c5-4d4e-7f8a-9b0c1d2e3f4a";
    public const string AnimalNotFosteredId = "e1f2a3b4-c5d6-4e5f-8a9b-0c1d2e3f4a5b";
    public const string AnimalShelterTestId = "f1a2b3c4-d5e6-4f7a-8b9c-0d1e2f3a4b5c";
    
    // Cancel fostering test animals
    public const string AnimalC1Id = "a5e6f7a8-b9c0-4d1e-2f3a-4b5c6d7e8f9a";
    public const string AnimalC2Id = "b6f7a8b9-c0d1-4e2f-3a4b-5c6d7e8f9a0b";
    public const string AnimalC3Id = "c7a8b9c0-d1e2-4f3a-4b5c-6d7e8f9a0b1c";
    public const string AnimalC4Id = "d8b9c0d1-e2f3-4a4b-5c6d-7e8f9a0b1c2d";
    public const string AnimalC5Id = "e9c0d1e2-f3a4-4b5c-6d7e-8f9a0b1c2d3e";
    public const string AnimalC6Id = "f0d1e2f3-a4b5-4c6d-7e8f-9a0b1c2d3e4f";
    public const string AnimalC7Id = "a1e2f3a4-b5c6-4d7e-8f9a-0b1c2d3e4f5a";
    public const string AnimalC8Id = "b2f3a4b5-c6d7-4e8f-9a0b-1c2d3e4f5a6b";

    // ======== USER IDS ========
    public const string User1Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
    public const string User2Id = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
    public const string User3Id = "cccccccc-cccc-cccc-cccc-cccccccccccc";
    public const string User4Id = "dddddddd-dddd-dddd-dddd-dddddddddddd";
    public const string User5Id = "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee";
    public const string User6Id = "66666666-6666-6666-6666-666666666666";
    public const string User7Id = "77777777-7777-7777-7777-777777777777";
    public const string User8Id = "88888888-8888-8888-8888-888888888888";
    public const string User9Id = "99999999-9999-9999-9999-999999999999";
    
    // Ownership request test users
    public const string OwnershipUser1Id = "user-1";
    public const string OwnershipUser2Id = "user-2";
    public const string OwnershipUser3Id = "user-3";
    
    // Fostering test users
    public const string FosterUserId = "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d";
    public const string RegularUserId = "b2c3d4e5-f6a7-4b6c-9d0e-1f2a3b4c5d6e";
    
    // Cancel fostering test users
    public const string CancelFosterUserId = "c1a2b3c4-d5e6-4f7a-8b9c-0d1e2f3a4b5c";
    public const string OtherCancelUserId = "d2b3c4d5-e6f7-4a8b-9c0d-1e2f3a4b5c6d";
    
    // Passwords
    public const string Password1 = "Pa$$w0rd";
    public const string Password2 = "Test@123";
    

    // ======== IMAGE URLS - SHELTERS ========
    public const string ImageShelterUrl1_1 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835433/shelter_qnix0r.jpg";
    public const string ImageShelterUrl1_2 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835501/shelter_lvjzl4.jpg";
    public const string ImageShelterUrl2_1 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835423/shelter_pypelc.jpg";
    public const string ImageShelterUrl2_2 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835552/shelter_q44gwo.jpg";

    // ======== IMAGE URLS - ANIMALS ========
    public const string ImageUrl1_1 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835037/image2_gjkcko.jpg";
    public const string ImageUrl1_2 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835034/images_fcbmbh.jpg";
    public const string ImageUrl2_1 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835062/image2_da9jlw.jpg";
    public const string ImageUrl2_2 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835058/images_t0jnkr.jpg";
    public const string ImageUrl3_1 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835079/image2_fcck0q.jpg";
    public const string ImageUrl3_2 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835075/images_jfawej.jpg";
    public const string ImageUrl4_1 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835089/image2_qnjamf.jpg";
    public const string ImageUrl4_2 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835085/images_jofy7m.jpg";
    public const string ImageUrl5_1 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835098/image2_pxn6g2.jpg";
    public const string ImageUrl5_2 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835093/images_rn6vpn.jpg";
    public const string ImageUrl6_1 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761834737/image2_rugk8b.jpg";
    public const string ImageUrl6_2 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761834219/images_jop2o1.jpg";
    public const string ImageUrl7_1 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835978/images_mn2jce.jpg";
    public const string ImageUrl7_2 = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835981/image2_kk3max.jpg";

    // ======== PUBLIC IDS - SHELTERS ========
    public const string PublicIdShelter1Img1 = "shelter_qnix0r";
    public const string PublicIdShelter1Img2 = "shelter_lvjzl4";
    public const string PublicIdShelter2Img1 = "shelter_pypelc";
    public const string PublicIdShelter2Img2 = "shelter_q44gwo";

    // ======== PUBLIC IDS - ANIMALS ========
    public const string PublicIdAnimal1Img1 = "image2_gjkcko";
    public const string PublicIdAnimal1Img2 = "images_fcbmbh";
    public const string PublicIdAnimal2Img1 = "image2_da9jlw";
    public const string PublicIdAnimal2Img2 = "images_t0jnkr";
    public const string PublicIdAnimal3Img1 = "image2_fcck0q";
    public const string PublicIdAnimal3Img2 = "images_jfawej";
    public const string PublicIdAnimal4Img1 = "image2_qnjamf";
    public const string PublicIdAnimal4Img2 = "images_jofy7m";
    public const string PublicIdAnimal5Img1 = "image2_pxn6g2";
    public const string PublicIdAnimal5Img2 = "images_rn6vpn";
    public const string PublicIdAnimal6Img1 = "image2_rugk8b";
    public const string PublicIdAnimal6Img2 = "images_jop2o1";
    public const string PublicIdAnimal7Img1 = "images_mn2jce";
    public const string PublicIdAnimal7Img2 = "image2_kk3max";

    // ======== IMAGE IDS - SHELTERS ========
    public const string ImageShelter1Img1Id = "00000000-0000-0000-0000-000000000101";
    public const string ImageShelter1Img2Id = "00000000-0000-0000-0000-000000000102";
    public const string ImageShelter2Img1Id = "00000000-0000-0000-0000-000000000201";
    public const string ImageShelter2Img2Id = "00000000-0000-0000-0000-000000000202";
    
    // Ownership request test shelter images
    public const string OwnershipShelterImg1Id = "shelter-img-1";
    public const string OwnershipShelterImg2Id = "shelter-img-2";

    // ======== IMAGE IDS - ANIMALS ========
    public const string ImageAnimal1Img1Id = "00000000-0000-0000-0000-000000001101";
    public const string ImageAnimal1Img2Id = "00000000-0000-0000-0000-000000001102";
    public const string ImageAnimal2Img1Id = "00000000-0000-0000-0000-000000002101";
    public const string ImageAnimal2Img2Id = "00000000-0000-0000-0000-000000002102";
    public const string ImageAnimal3Img1Id = "00000000-0000-0000-0000-000000003101";
    public const string ImageAnimal3Img2Id = "00000000-0000-0000-0000-000000003102";
    public const string ImageAnimal4Img1Id = "00000000-0000-0000-0000-000000004101";
    public const string ImageAnimal4Img2Id = "00000000-0000-0000-0000-000000004102";
    public const string ImageAnimal5Img1Id = "00000000-0000-0000-0000-000000005101";
    public const string ImageAnimal5Img2Id = "00000000-0000-0000-0000-000000005102";
    public const string ImageAnimal6Img1Id = "00000000-0000-0000-0000-000000006101";
    public const string ImageAnimal6Img2Id = "00000000-0000-0000-0000-000000006102";
    public const string ImageAnimal7Img1Id = "00000000-0000-0000-0000-000000007101";
    public const string ImageAnimal7Img2Id = "00000000-0000-0000-0000-000000007102";
    public const string ImageAnimal12Img1Id = "00000000-0000-0000-0000-000000012101";
    public const string ImageAnimal12Img2Id = "00000000-0000-0000-0000-000000012102";
    public const string ImageAnimal13Img1Id = "00000000-0000-0000-0000-000000013101";
    public const string ImageAnimal13Img2Id = "00000000-0000-0000-0000-000000013102";
    public const string ImageAnimal14Img1Id = "00000000-0000-0000-0000-000000014101";
    public const string ImageAnimal14Img2Id = "00000000-0000-0000-0000-000000014102";
    public const string ImageAnimal15Img1Id = "00000000-0000-0000-0000-000000015101";
    public const string ImageAnimal15Img2Id = "00000000-0000-0000-0000-000000015102";
    
    // Ownership request test animal images
    public const string OwnershipAnimalImg1Id = "img-1";
    public const string OwnershipAnimalImg2Id = "img-2";
    public const string OwnershipAnimalImg3Id = "img-3";
    public const string OwnershipAnimalImg4Id = "img-4";
    public const string OwnershipAnimalImg5Id = "img-5";
    public const string OwnershipAnimalImg6Id = "img-6";
    public const string OwnershipAnimalImg7Id = "img-7";
    public const string OwnershipAnimalImg8Id = "img-8";
    public const string OwnershipAnimalImg9Id = "img-9";

    // ======== FAVORITE IDS ========
    public const string Favorite1Id = "fav00000-0000-0000-0000-000000000001";
    public const string Favorite2Id = "fav00000-0000-0000-0000-000000000002";
    public const string Favorite3Id = "fav00000-0000-0000-0000-000000000003";
    
    // ======== OWNERSHIP REQUEST IDS ========
    public const string OwnershipRequest1Id = "or-1";
    public const string OwnershipRequest2Id = "or-2";
    public const string OwnershipRequest3Id = "or-3";
    public const string OwnershipRequest4Id = "or-4";
    public const string OwnershipRequest5Id = "or-5";
}