using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.AdvisorBankAccounts
{
    public class AdvisorBankAccountService : IAdvisorBankAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;

        public AdvisorBankAccountService(
            IUnitOfWork unitOfWork,
            IUserService userService,
            ISieveProcessor sieveProcessor,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
        }

        public async Task<PagedResult<AdvisorBankAccountResponse>> GetAllAsync(SieveModel model)
        {
            return await PaginationHelper.PaginateAsync(
                _unitOfWork.AdvisorBankAccounts.GetAllQuery(),
                model,
                _sieveProcessor,
                x => _mapper.Map<AdvisorBankAccountResponse>(x));
        }

        public async Task<AdvisorBankAccountResponse?> GetByIdAsync(int id)
        {
            var account = await _unitOfWork.AdvisorBankAccounts.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Advisor bank account not found.");

            var role = _userService.GetUserRole();
            if (string.Equals(role, "Advisor", StringComparison.OrdinalIgnoreCase))
            {
                var currentUserId = _userService.GetUserId();
                if (account.Advisor.UserId != currentUserId)
                    throw new ForbiddenAccessException("You do not have permission to view this bank account.");
            }

            return _mapper.Map<AdvisorBankAccountResponse>(account);
        }

        public async Task<AdvisorBankAccountResponse?> GetMyAsync()
        {
            var currentUserId = _userService.GetUserId();
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(currentUserId)
                ?? throw new KeyNotFoundException("Advisor profile not found.");

            var account = await _unitOfWork.AdvisorBankAccounts.GetActiveByAdvisorIdAsync(advisor.AdvisorId)
                ?? throw new KeyNotFoundException("Active advisor bank account not found.");

            return _mapper.Map<AdvisorBankAccountResponse>(account);
        }

        public async Task<AdvisorBankAccountResponse> CreateAsync(CreateAdvisorBankAccountRequest request)
        {
            var currentUserId = _userService.GetUserId();
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(currentUserId)
                ?? throw new KeyNotFoundException("Advisor profile not found.");
            EnsureAdvisorApproved(advisor);

            var existingActive = await _unitOfWork.AdvisorBankAccounts.GetActiveByAdvisorIdAsync(advisor.AdvisorId);
            if (existingActive is not null)
                throw new InvalidOperationException("Active bank account already exists. Please use update.");

            var bankName = request.BankName.Trim();
            var accountNumber = request.AccountNumber.Trim();
            var accountHolderName = request.AccountHolderName.Trim();

            var account = new AdvisorBankAccount
            {
                AdvisorId = advisor.AdvisorId,
                BankName = bankName,
                AccountNumber = accountNumber,
                AccountHolderName = accountHolderName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.AdvisorBankAccounts.AddAsync(account);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.AdvisorBankAccounts.GetByIdAsync(account.AdvisorBankAccountId)
                ?? throw new KeyNotFoundException("Advisor bank account not found.");

            return _mapper.Map<AdvisorBankAccountResponse>(created);
        }

        public async Task<AdvisorBankAccountResponse> UpdateAsync(int id, UpdateAdvisorBankAccountRequest request)
        {
            var account = await _unitOfWork.AdvisorBankAccounts.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Advisor bank account not found.");

            var currentUserId = _userService.GetUserId();
            if (account.Advisor.UserId != currentUserId)
                throw new ForbiddenAccessException("You do not have permission to update this bank account.");
            EnsureAdvisorApproved(account.Advisor);

            account.BankName = request.BankName.Trim();
            account.AccountNumber = request.AccountNumber.Trim();
            account.AccountHolderName = request.AccountHolderName.Trim();
            account.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.AdvisorBankAccounts.Update(account);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AdvisorBankAccountResponse>(account);
        }

        public async Task<AdvisorBankAccountResponse> DeactivateAsync(int id)
        {
            var account = await _unitOfWork.AdvisorBankAccounts.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Advisor bank account not found.");

            var role = _userService.GetUserRole();
            if (string.Equals(role, "Advisor", StringComparison.OrdinalIgnoreCase))
            {
                var currentUserId = _userService.GetUserId();
                if (account.Advisor.UserId != currentUserId)
                    throw new ForbiddenAccessException("You do not have permission to deactivate this bank account.");
                EnsureAdvisorApproved(account.Advisor);
            }

            if (!account.IsActive)
            {
                return _mapper.Map<AdvisorBankAccountResponse>(account);
            }

            account.IsActive = false;
            account.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.AdvisorBankAccounts.Update(account);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AdvisorBankAccountResponse>(account);
        }

        private static void EnsureAdvisorApproved(Advisor advisor)
        {
            if (advisor.ApprovalStatus != ApprovalStatus.Approved)
                throw new InvalidOperationException("Your advisor profile must be approved before using this feature.");
        }
    }
}
