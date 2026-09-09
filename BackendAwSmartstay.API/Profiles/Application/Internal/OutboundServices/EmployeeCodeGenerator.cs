using System;
using System.Data;
using System.Threading.Tasks;
using BackendAwSmartstay.API.Profiles.Infrastructure.Persistence.EFC.Entities;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Profiles.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BackendAwSmartstay.API.Profiles.Application.Internal.OutboundServices;

/// <summary>
/// Infrastructure outbound service for generating unique, sequential employee codes (EMP-XXXXX).
/// Executes an atomic database operation against MySQL/MariaDB to guarantee concurrency safety
/// across multiple server instances without in-memory locks or table scanning.
/// </summary>
public class EmployeeCodeGenerator(AppDbContext context) : IEmployeeCodeGenerator
{
    public async Task<EmployeeCode> GenerateNextCodeAsync()
    {
        if (context.Database.IsRelational())
        {
            var connection = context.Database.GetDbConnection();
            bool wasClosed = connection.State == ConnectionState.Closed;
            if (wasClosed)
            {
                await connection.OpenAsync();
            }

            try
            {
                using var command = connection.CreateCommand();
                var currentTx = context.Database.CurrentTransaction?.GetDbTransaction();
                if (currentTx != null)
                {
                    command.Transaction = currentTx;
                }

                command.CommandText = @"
                    INSERT INTO `employee_code_sequences` (`id`, `last_value`) VALUES (1, 0)
                    ON DUPLICATE KEY UPDATE `id` = `id`;
                    UPDATE `employee_code_sequences` 
                    SET `last_value` = LAST_INSERT_ID(`last_value` + 1) 
                    WHERE `id` = 1;
                    SELECT LAST_INSERT_ID();";

                var scalarResult = await command.ExecuteScalarAsync();
                if (scalarResult is null || scalarResult is DBNull)
                {
                    throw new InvalidOperationException("Failed to generate employee code from relational database sequence.");
                }

                long nextVal = Convert.ToInt64(scalarResult);
                return new EmployeeCode($"EMP-{nextVal:D5}");
            }
            finally
            {
                if (wasClosed && context.Database.CurrentTransaction == null)
                {
                    await connection.CloseAsync();
                }
            }
        }
        else
        {
            // Fallback for non-relational test environments (e.g., InMemory provider)
            var sequence = await context.EmployeeCodeSequences.FindAsync(1);
            if (sequence is null)
            {
                sequence = new EmployeeCodeSequence { Id = 1, LastValue = 0 };
                context.EmployeeCodeSequences.Add(sequence);
            }

            sequence.LastValue++;
            await context.SaveChangesAsync();

            return new EmployeeCode($"EMP-{sequence.LastValue:D5}");
        }
    }
}
