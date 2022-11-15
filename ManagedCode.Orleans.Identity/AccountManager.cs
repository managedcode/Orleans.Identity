// using System;
// using System.Threading.Tasks;
// using ManagedCode.Communication;
//
// namespace ManagedCode.Orleans.Identity;
//
// public class AccountManager
// {
//     private readonly IAccountRepository _accountRepository;
//
//     public AccountManager(IAccountRepository repository)
//     {
//         _accountRepository = repository;
//     }
//
//     public async Task<Result> CreateAsync(AccountEntity account)
//     {
//         if (account is null)
//         {
//             throw new ArgumentNullException(nameof(account));
//         }
//
//         await _accountRepository.InsertAsync(account);
//
//         return Result.Succeed();
//     }
//
//     public async Task<Result> UpdateAsync(AccountEntity account)
//     {
//         if (account is null)
//         {
//             throw new ArgumentNullException(nameof(account));
//         }
//
//         await _accountRepository.UpdateAsync(account);
//
//         return Result.Succeed();
//     }
//
//     public Task AddToRoleAsync(AccountEntity account, Role role)
//     {
//         if (account is null)
//         {
//             throw new ArgumentNullException(nameof(account));
//         }
//
//         if (!account.Roles.Contains(role))
//         {
//             account.Roles.Add(role);
//         }
//
//         return Task.CompletedTask;
//     }
//
//     public async Task<AccountEntity> FindByEmailAsync(string email)
//     {
//         if (email is null)
//         {
//             throw new ArgumentNullException(nameof(email));
//         }
//
//         var account = await _accountRepository.GetAsync(u => u.Email == email);
//
//         return account;
//     }
//
//     public async Task<AccountEntity> FindByIdAsync(string id)
//     {
//         if (id is null)
//         {
//             throw new ArgumentNullException(nameof(id));
//         }
//
//         var account = await _accountRepository.GetAsync(id);
//
//         return account;
//     }
//
//     public async Task<AccountEntity> FindByPhoneAsync(string phone)
//     {
//         if (phone is null)
//         {
//             throw new ArgumentNullException(nameof(phone));
//         }
//
//         var account = await _accountRepository.GetAsync(u => u.Phone == phone);
//
//         // Crutch for germany phone numbers
//         if (account is null && phone.StartsWith("+49"))
//         {
//             phone = phone.StartsWith("+490")
//                 ? $"+49{phone[4..]}"
//                 : $"+490{phone[3..]}";
//
//             account = await _accountRepository.GetAsync(u => u.Phone == phone);
//         }
//
//         return account;
//     }
//
//     public async Task DeleteAsync(AccountEntity account)
//     {
//         if (account is null)
//         {
//             throw new ArgumentNullException(nameof(account));
//         }
//
//         await _accountRepository.DeleteAsync(account);
//     }
//
//
//     public async Task<bool> IsPhoneExistAsync(string phone)
//     {
//         if (phone is null)
//         {
//             throw new ArgumentNullException(nameof(phone));
//         }
//
//         var account = await _accountRepository.GetAsync(u => u.Phone == phone);
//
//         return account is not null;
//     }
// }