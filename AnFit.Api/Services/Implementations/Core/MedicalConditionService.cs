using AnFit.Api.DTOs.Responses;
using AnFit.Api.Exceptions;
using AnFit.Api.Extensions;
using AnFit.Api.Models.Entities;
using AnFit.Api.Repositories.Interfaces;
using AnFit.Api.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Implementations.Core
{
    public class MedicalConditionService : IMedicalConditionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MedicalConditionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<MedicalConditionResponse>> GetAllAsync()
        {
            var conditions = await _unitOfWork.MedicalConditions.GetAllAsync();
            return conditions.Select(c => c.ToResponse());
        }

        public async Task<MedicalConditionResponse?> GetByIdAsync(int id)
        {
            var condition = await _unitOfWork.MedicalConditions.GetByIdAsync(id);
            if (condition == null) throw new NotFoundException("Không tìm thấy bệnh lý.");
            return condition.ToResponse();
        }

        public async Task<IEnumerable<MedicalConditionResponse>> GetByTypeAsync(int type)
        {
            var conditions = await _unitOfWork.MedicalConditions.GetByTypeAsync(type);
            return conditions.Select(c => c.ToResponse());
        }

        public async Task<MedicalConditionResponse> CreateAsync(string conditionName, int type)
        {
            var newCondition = new MedicalCondition
            {
                ConditionName = conditionName,
                Type = type
            };

            await _unitOfWork.MedicalConditions.AddAsync(newCondition);
            await _unitOfWork.SaveChangesAsync();

            return newCondition.ToResponse();
        }

        public async Task UpdateAsync(int id, string conditionName, int type)
        {
            var condition = await _unitOfWork.MedicalConditions.GetByIdAsync(id);
            if (condition == null) throw new NotFoundException("Không tìm thấy bệnh lý.");

            condition.ConditionName = conditionName;
            condition.Type = type;

            _unitOfWork.MedicalConditions.Update(condition);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var condition = await _unitOfWork.MedicalConditions.GetByIdAsync(id);
            if (condition == null) throw new NotFoundException("Không tìm thấy bệnh lý.");

            _unitOfWork.MedicalConditions.Remove(condition);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}