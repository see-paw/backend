namespace API.DTOs
{
    /// <summary>
    /// Represents the data transfer object (DTO) used when creating a new animal record.
    /// </summary>
    /// <remarks>
    /// Inherits all common animal properties from <see cref="BaseAnimalDTO"/>.
    /// This DTO can be extended in the future with fields specific to animal creation,
    /// such as shelter identification or metadata provided by the user.
    /// </remarks>
    public class CreateAnimalDTO : BaseAnimalDTO
    {
    }
}
