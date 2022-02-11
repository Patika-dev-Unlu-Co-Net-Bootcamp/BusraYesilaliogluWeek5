using LightQuery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnluCoWeek5.Models;
using UnluCoWeek5.Repository;

namespace UnluCoWeek5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase

    {

        private readonly MemberContext _context;
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;
        private readonly IMemberRepository _memberRepository;

        public MembersController(IMemberRepository memberRepository, IMemoryCache memoryCache, IDistributedCache distributedCache, MemberContext context)
        {

            _memberRepository = memberRepository;
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
            _context = context;

        }

        //Distributed Cache
        [HttpGet("dCache")]
        public IEnumerable<Member> GetMember([FromQuery] Models.QueryParams query)
        {
            var cachedData = _distributedCache.GetString(key: "memberData");
            var data = _memberRepository.GetMembers(query);
            if (string.IsNullOrEmpty(cachedData))
            {

                var cacheOptions = new DistributedCacheEntryOptions()
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(15),

                };
                _distributedCache.SetString(key: "memberData", value: JsonConvert.SerializeObject(data), cacheOptions);
                return data;
            }
            else
            {
                return JsonConvert.DeserializeObject<IEnumerable<Member>>(cachedData);
            }
            return data;
        }

        //Response Cache
        [LightQuery]
        [HttpGet("rCache")]
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, NoStore = false, VaryByQueryKeys = new string[] { "page:" })]
        public IActionResult GetMembers([FromQuery] Models.QueryParams query)
        {

            var list = _memberRepository.GetMembers(query);

            Response.Headers.Add("X-Paging", System.Text.Json.JsonSerializer.Serialize(list.Result));
            return Ok(new { data = list, paging = list.Result });
        }

        //In-Memory Cache
        [HttpGet("mCache")]
        public IActionResult GetMemb([FromQuery] Models.QueryParams query, string exap)
        {


            if (_memoryCache.TryGetValue("members", out List<Member> member))
            {
                return Ok(member);
            }
            var list = _memberRepository.GetMembers(query);
            _memoryCache.Set("members", list, new MemoryCacheEntryOptions
            {

                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.Normal

            });
            Response.Headers.Add("X-Paging", System.Text.Json.JsonSerializer.Serialize(list.Result));
            return Ok(new { data = list, paging = list.Result, exap = "ÖRNEK ÜYE LİSTESİDİR." });
        }




        //Filtreleme:  ?card=jcb    localhost:5000/api/members/all?card=jcb
        //Listeleme: ?sort=lastname&dir=asc  localhost:5000/api/members/all?sort=lastname&dir=asc
        //Veri sayısını limitleme: ?limit=15   localhost:5000/api/members/all?limit=35
        //Arama: ?search=alicia  localhost:5000/api/members/all?search=alicia
        [Produces("application/json")]
        [HttpGet("all")]
        // Evet burası berbat ve karışık. Ama herşeyi denedim hep hata aldım.(IMapper ya da yukarıda ki gibi)
        public List<Member> GetAllItems(
           string name,
           string lastname,
           string email,
           string card,
           string search,
           int? page,
           string sort,
           int limit = 25,
           string dir = "desc")
        {
            IQueryable<Member> query = _context.MOCK_DATA;

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(d => d.FirstName.Contains(name));
            if (!string.IsNullOrWhiteSpace(lastname))
                query = query.Where(d => d.LastName == lastname);
            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(d => d.Email == email);
            if (!string.IsNullOrWhiteSpace(card))
                query = query.Where(d => d.CreditCardType == card);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(d => d.CreditCardType == search || d.Email == search || d.LastName == search || d.FirstName == search);

            if (!string.IsNullOrWhiteSpace(sort))
            {
                switch (sort)
                {
                    case "name":
                        if (dir == "asc")
                            query = query.OrderBy(d => d.FirstName);
                        else if (dir == "desc")
                            query = query.OrderByDescending(d => d.FirstName);
                        break;
                    case "email":
                        if (dir == "asc")
                            query = query.OrderBy(d => d.Email);
                        else if (dir == "desc")
                            query = query.OrderByDescending(d => d.Email);
                        break;
                    case "lastname":
                        if (dir == "asc")
                            query = query.OrderBy(d => d.LastName);
                        else if (dir == "desc")
                            query = query.OrderByDescending(d => d.LastName);
                        break;
                    case "card":
                        if (dir == "asc")
                            query = query.OrderBy(d => d.CreditCardType);
                        else if (dir == "desc")
                            query = query.OrderByDescending(d => d.CreditCardType);
                        break;
                }
            }
            query = query.Take(limit);
            if (page.HasValue)
                query = query.Skip(page.Value * limit);

            return query.ToList();
        }


    }
}
