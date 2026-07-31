using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CogStayMVC.DTOs;
using CogStayMVC.Enums;
using CogStayMVC.Models;
using CogStayMVC.Repositories.Interfaces;
using CogStayMVC.Services.Interfaces;
using TaskStatus = CogStayMVC.Enums.TaskStatus;

namespace CogStayMVC.Services.Housekeeping;

public class HousekeepingService : IHousekeepingService
{
    private readonly IHousekeepingTaskRepository _taskRepository;
    private readonly IRoomRepository _roomRepository;

    public HousekeepingService(
        IHousekeepingTaskRepository taskRepository,
        IRoomRepository roomRepository)
    {
        _taskRepository = taskRepository;
        _roomRepository = roomRepository;
    }

    public async Task<IEnumerable<HousekeepingTaskResponseDTO>> GetAllTasksAsync()
    {
        var tasks = await _taskRepository.GetTasksWithDetailsAsync();
        return tasks.Select(MapToDTO);
    }

    public async Task<HousekeepingTaskResponseDTO?> GetTaskByIdAsync(int id)
    {
        var task = await _taskRepository.GetTaskWithDetailsAsync(id);
        return task != null ? MapToDTO(task) : null;
    }

    public async Task<IEnumerable<HousekeepingTaskResponseDTO>> GetTasksByRoomIdAsync(int roomId)
    {
        var tasks = await _taskRepository.GetTasksByRoomIdAsync(roomId);
        return tasks.Select(MapToDTO);
    }

    public async Task<HousekeepingTaskResponseDTO> CreateTaskAsync(CreateHousekeepingTaskDTO dto)
    {
        var room = await _roomRepository.GetByIdAsync(dto.RoomId);
        if (room == null)
            throw new InvalidOperationException("Room not found.");

        var task = new HousekeepingTask
        {
            RoomId = dto.RoomId,
            TaskDescription = dto.TaskDescription,
            TaskStatus = TaskStatus.Pending
        };

        await _taskRepository.AddAsync(task);
        var created = await _taskRepository.GetTaskWithDetailsAsync(task.TaskId);
        return MapToDTO(created ?? task);
    }

    public async Task UpdateTaskStatusAsync(UpdateTaskStatusDTO dto)
    {
        var task = await _taskRepository.GetTaskWithDetailsAsync(dto.TaskId);
        if (task == null)
            throw new KeyNotFoundException("Housekeeping task not found.");

        task.TaskStatus = dto.TaskStatus;
        await _taskRepository.UpdateAsync(task);

        // Room status state machine update
        if (task.Room != null)
        {
            if (dto.TaskStatus == TaskStatus.InProgress)
            {
                task.Room.Status = RoomStatus.CleaningInProgress;
                await _roomRepository.UpdateAsync(task.Room);
            }
            else if (dto.TaskStatus == TaskStatus.Completed)
            {
                task.Room.Status = RoomStatus.Available; // Visible again for public booking!
                await _roomRepository.UpdateAsync(task.Room);
            }
        }
    }

    public async Task DeleteTaskAsync(int id)
    {
        await _taskRepository.DeleteAsync(id);
    }

    private static HousekeepingTaskResponseDTO MapToDTO(HousekeepingTask task) => new()
    {
        TaskId = task.TaskId,
        RoomId = task.RoomId,
        RoomNumber = task.Room?.RoomNumber ?? "N/A",
        TaskDescription = task.TaskDescription,
        TaskStatus = task.TaskStatus
    };
}
