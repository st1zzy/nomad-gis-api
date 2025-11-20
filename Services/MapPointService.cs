using AutoMapper;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Points;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Services
{
    public class MapPointService : IMapPointService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // <-- 1. Добавляем IMapper

        // 2. Внедряем IMapper через конструктор
        public MapPointService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<MapPointRequest>> GetAllAsync()
        {
            var points = await _context.MapPoints.ToListAsync();
            // 3. Используем маппинг
            return _mapper.Map<List<MapPointRequest>>(points);
        }

        public async Task<MapPointRequest?> GetByIdAsync(Guid id)
        {
            var mp = await _context.MapPoints.FindAsync(id);
            if (mp == null) return null;
            
            // 3. Используем маппинг
            return _mapper.Map<MapPointRequest>(mp);
        }

        public async Task<MapPointRequest> CreateAsync(MapPointCreateRequest dto)
        {
            // 3. Используем маппинг
            var mp = _mapper.Map<MapPoint>(dto);
            
            // Устанавливаем значения по умолчанию, если они не в DTO
            mp.Id = Guid.NewGuid();
            mp.CreatedAt = DateTime.UtcNow;

            _context.MapPoints.Add(mp);
            await _context.SaveChangesAsync();
            
            // 3. Используем маппинг
            return _mapper.Map<MapPointRequest>(mp);
        }

        public async Task<MapPointRequest?> UpdateAsync(Guid id, MapPointUpdateRequest dto)
        {
            var mp = await _context.MapPoints.FindAsync(id);
            if (mp == null) return null;

            // 3. Используем AutoMapper для обновления существующей сущности
            _mapper.Map(dto, mp);

            _context.MapPoints.Update(mp);
            await _context.SaveChangesAsync();

            return _mapper.Map<MapPointRequest>(mp);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var mp = await _context.MapPoints.FindAsync(id);
            if (mp == null) return false;

            _context.MapPoints.Remove(mp);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}