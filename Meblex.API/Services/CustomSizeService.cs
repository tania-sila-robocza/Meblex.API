﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AgileObjects.AgileMapper;
using Dawn;
using Meblex.API.Context;
using Meblex.API.FormsDto.Request;
using Meblex.API.FormsDto.Response;
using Meblex.API.Helper;
using Meblex.API.Interfaces;
using Meblex.API.Models;
using Microsoft.Extensions.Localization;

namespace Meblex.API.Services
{
    public class CustomSizeService:ICustomSizeService
    {
        private readonly MeblexDbContext _context;
        private readonly IFurnitureService _furnitureService;
        private readonly IStringLocalizer<CustomSizeService> _localizer;

        public CustomSizeService(MeblexDbContext context, IFurnitureService furnitureService, IStringLocalizer<CustomSizeService> localizer)
        {
            _localizer = localizer;
            _context = context;
            _furnitureService = furnitureService;
        }
        public int AddCustomSize(CustomSizeAddFrom form, int userId)
        {
            var toAdd = Mapper.Map(form).ToANew<CustomSizeForm>();
            toAdd.Client = _context.Users.Find(userId).Client ??
                           throw new HttpStatusCodeException(HttpStatusCode.NotFound, _localizer["Could not find client"]);
            toAdd.PieceOfFurniture = _context.Furniture.Find(form.PieceOfFurnitureId) ??
                                     throw new HttpStatusCodeException(HttpStatusCode.NotFound, _localizer["Could not find furniture part"]);
            toAdd.Approved = false;
            _context.CustomSizeForms.Add(toAdd);

            if (_context.SaveChanges() == 0)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, _localizer["Could not add data"]);
            }

            return toAdd.CustomSizeFormId;
        }

        public CustomSizeFormResponse GetById(int id)
        {
            var Id = Guard.Argument(id, nameof(id)).NotNegative().NotZero().Value;

            var customSizeForm = _context.CustomSizeForms.Find(id) ??
                                 throw new HttpStatusCodeException(HttpStatusCode.NotFound, _localizer["Could not find individual size"]);
            var response = Mapper.Map(customSizeForm).ToANew<CustomSizeFormResponse>();
            response.PieceOfFurniture = _furnitureService.GetPieceOfFurniture(customSizeForm.PieceOfFurnitureId);

            return response;

        }

        public List<CustomSizeFormResponse> GetAllClientForms(int userId)
        {
            var UserId = Guard.Argument(userId, nameof(userId)).NotNegative().NotZero().Value;

            var user = _context.Users.Find(UserId) ??
                       throw new HttpStatusCodeException(HttpStatusCode.NotFound, _localizer["User not found"]);
            var client = user.Client ??
                         throw new HttpStatusCodeException(HttpStatusCode.NotFound, _localizer["User found"]);
            var forms = client.CustomSizeForms;
            if(forms == null) return new List<CustomSizeFormResponse>();
            var response = new List<CustomSizeFormResponse>();

            foreach (var form in forms)
            {
                var toAdd = Mapper.Map(form).ToANew<CustomSizeFormResponse>();
                var pieceOfFurniture = _furnitureService.GetPieceOfFurniture(form.PieceOfFurnitureId);
                toAdd.PieceOfFurniture = pieceOfFurniture;
                response.Add(toAdd);
            }

            return response;
        }

        public CustomSizeFormResponse GetClientFormById(int id, int userId)
        {
            var Id = Guard.Argument(id, nameof(id)).NotNegative().NotZero().Value;
            var UserId = Guard.Argument(userId, nameof(userId)).NotNegative().NotZero().Value;

            var customSizeForm = _context.CustomSizeForms.Single(x => x.CustomSizeFormId == Id && x.Client.UserId == UserId) ??
                                 throw new HttpStatusCodeException(HttpStatusCode.NotFound, _localizer["Individual size not found"]);
            var response = Mapper.Map(customSizeForm).ToANew<CustomSizeFormResponse>();
            response.PieceOfFurniture = _furnitureService.GetPieceOfFurniture(customSizeForm.PieceOfFurnitureId);

            return response;

        }

        public CustomSizeFormResponse ApproveCustomSizeForm(int id, float price)
        {
            var Id = Guard.Argument(id, nameof(id)).Value;
            var Price = Guard.Argument(price, nameof(price)).Value;

            var form = _context.CustomSizeForms.Find(Id) ??
                       throw new HttpStatusCodeException(HttpStatusCode.NotFound, _localizer["Individual size not found"]);
            if(form.Approved) throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, _localizer["Accepted"]);
            form.Approved = true;
            form.Price = Price;

            _context.CustomSizeForms.Update(form);

            if (_context.SaveChanges() == 0)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, _localizer["Could not add data"]);
            }

            return GetById(form.CustomSizeFormId);
        }

        public List<CustomSizeFormResponse> GetAllCustomSizeForm()
        {
            var forms = _context.CustomSizeForms?.Where(x => x.Approved == false);
            if (forms == null || forms.ToList().Count == 0)
            {
                return new List<CustomSizeFormResponse>();
            }

            var response = new List<CustomSizeFormResponse>();
            foreach (var form in forms)
            {
                var toAdd = Mapper.Map(form).ToANew<CustomSizeFormResponse>();
                var pieceOfFurniture = _furnitureService.GetPieceOfFurniture(form.PieceOfFurnitureId);
                toAdd.PieceOfFurniture = pieceOfFurniture;
                response.Add(toAdd);
            }

            return response;

        }
    }
}
