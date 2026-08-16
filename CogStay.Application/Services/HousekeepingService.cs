using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CogStay.Application.Contracts.Persistence;
using CogStay.Application.Contracts.Services;
using CogStay.Application.DTOs;
using CogStay.Domain.Entities;
using CogStay.Domain.Enums;
using TaskStatus = CogStay.Domain.Enums.TaskStatus;

namespace CogStay.Application.Services;

public class HousekeepingService : IHousekeepingService
{
    private readonly IHousekeepingTaskRepository _taskRepository;
    private readonly IRoomRepository _roomRepository;

    public HousekeepingService(IHousekeepingTaskRepository taskRepository, IRoomRepository roomRepository)
    {
        _taskRepository = taskRepository;
        _roomRepository = roomRepository;
    }

    public async Task<IEnumerable<HousekeepingTaskResponseDTO>> GetAllTasksAsync()
    {
        var tasks = await _taskRepository.GetAllAsync();
        return await MapToDTOListAsync(tasks);
    }

    public async Task<HousekeepingTaskResponseDTO?> GetTaskByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return null;
        var room = await _roomRepository.GetByIdAsync(task.RoomId);
        return MapToDTO(task, room);
    }

    public async Task<IEnumerable<HousekeepingTaskResponseDTO>> GetTasksByRoomIdAsync(int roomId)
    {
        var tasks = await _taskRepository.GetByRoomIdAsync(roomId);
        return await MapToDTOListAsync(tasks);
    }

    public async Task<HousekeepingTaskResponseDTO> CreateTaskAsync(CreateHousekeepingTaskDTO dto)
    {
        var room = await _roomRepository.GetByIdAsync(dto.RoomId);
        if (room == null)
        {
            throw new KeyNotFoundException($"Room with ID {dto.RoomId} not found.");
        }

        var nextId = await _taskRepository.GetNextTaskIdAsync();
        var task = new HousekeepingTask
        {
            TaskId = nextId,
            RoomId = dto.RoomId,
            TaskDescription = dto.TaskDescription,
            TaskStatus = TaskStatus.Pending
        };

        await _taskRepository.CreateAsync(task);

        if (room.Status == RoomStatus.Available)
        {
            room.Status = RoomStatus.NeedsCleaning;
            await _roomRepository.UpdateAsync(room);
        }

        return MapToDTO(task, room);
    }

    public async Task UpdateTaskStatusAsync(UpdateTaskStatusDTO dto)
    {
        var task = await _taskRepository.GetByIdAsync(dto.TaskId);
        if (task == null)
        {
            throw new KeyNotFoundException($"Housekeeping task with ID {dto.TaskId} not found.");
        }

        task.TaskStatus = dto.TaskStatus;
        await _taskRepository.UpdateAsync(task);

        if (dto.TaskStatus == TaskStatus.Completed)
        {
            var room = await _roomRepository.GetByIdAsync(task.RoomId);
            if (room != null && (room.Status == RoomStatus.NeedsCleaning || room.Status == RoomStatus.Maintenance))
            {
                room.Status = RoomStatus.Available;
                await _roomRepository.UpdateAsync(room);
            }
        }
    }

    public async Task DeleteTaskAsync(int id)
    {
        await _taskRepository.DeleteAsync(id);
    }

    private async Task<IEnumerable<HousekeepingTaskResponseDTO>> MapToDTOListAsync(IEnumerable<HousekeepingTask> list)
    {
        var dtos = new List<HousekeepingTaskResponseDTO>();
        foreach (var t in list)
        {
            var room = await _roomRepository.GetByIdAsync(t.RoomId);
            dtos.Add(MapToDTO(t, room));
        }
        return dtos;
    }

    private static HousekeepingTaskResponseDTO MapToDTO(HousekeepingTask t, Room? r) => new()
    {
        TaskId = t.TaskId,
        RoomId = t.RoomId,
        RoomNumber = r?.RoomNumber ?? "N/A",
        TaskDescription = t.TaskDescription,
        TaskStatus = t.TaskStatus
    };
}
