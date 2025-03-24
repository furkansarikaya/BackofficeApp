namespace Backoffice.Web.Attributes;

// Attribute to mark properties that should be encrypted
[AttributeUsage(AttributeTargets.Property)]
public class EncryptAttribute : Attribute
{
}