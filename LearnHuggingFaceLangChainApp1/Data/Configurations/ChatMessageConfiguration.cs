using System.Text.Json;
using LearnHuggingFaceLangChainApp1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnHuggingFaceLangChainApp1.Data.Configurations;

public sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("chat_messages");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.SessionId)
            .HasColumnName("session_id")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Role)
            .HasColumnName("role")
            .HasConversion<string>()   // enum → string in db
            .HasMaxLength(16);

        builder.Property(c => c.Content)
            .HasColumnName("content")
            .IsRequired();

        builder.Property<JsonDocument?>(c => c.MetaData)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        builder.Property<DateTime?>(c => c.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(c => new { c.SessionId, c.CreatedAt })
            .HasDatabaseName("idx_chat_messages_session_time");
    }
}