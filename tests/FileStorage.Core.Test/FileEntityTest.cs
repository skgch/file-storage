namespace FileStorage.Core.Test;

[Parallelizable]
public class FileEntityTest
{
    [Test]
    public void IsValid_ReturnsTrueWhenValid()
    {
        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms);
        sw.Write("This is a test file.");
        
        var fileEntity = new FileEntity
        {
            FileName = "valid.text",
            Content = ms,
        };
        
        Assert.That(fileEntity.IsValid(), Is.True);
    }
    
    [Test]
    public void IsValid_ReturnsFalseWhenFileNameIsTooLong()
    {
        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms);
        sw.Write("This is a test file.");
        
        var fileEntity = new FileEntity
        {
            FileName = "12345678901234567890123456789012345678901234567890" +
                       "12345678901234567890123456789012345678901234567890" +
                       "12345678901234567890123456789012345678901234567890" +
                       "12345678901234567890123456789012345678901234567890" +
                       "12345678901234567890123456789012345678901234567890" +
                       "1",
            Content = ms,
        };
        
        Assert.That(fileEntity.IsValid(), Is.False);
    }
    
    [Test]
    public void IsValid_ReturnsFalseWhenContentIsTooLarge()
    {
        using var ms = new MemoryStream();
        ms.Seek(5 * 1024 * 1024, SeekOrigin.Begin);
        ms.WriteByte(0);

        var fileEntity = new FileEntity
        {
            FileName = "too_large.text",
            Content = ms,
        };
        
        Assert.That(fileEntity.IsValid(), Is.False);
    }
}