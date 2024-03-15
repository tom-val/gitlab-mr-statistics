using System.Text.Json.Serialization;

namespace get_mr_statistics.Models;

public record SlackMessage
{
    public string Text { get; set; }
    public List<Block> Blocks { get; set; }
}

[JsonDerivedType(typeof(HeaderBlock))]
[JsonDerivedType(typeof(SectionBlock))]
public abstract record Block(string Type);

public record HeaderBlock() : Block("header")
{
    public SimpleTextBlock Text { get; set; }
}

public record SectionBlock() : Block("section")
{
    public MarkdownBlock Text { get; set; }
    public List<MarkdownBlock> Fields { get; set; }
}

public record MarkdownBlock
{
    public string Type { get; } = "mrkdwn";
    public string Text { get; set; }
}

public record SimpleTextBlock
{
    public string Type { get; } = "plain_text";
    public string Text { get; set; }
}
