﻿using System;
using AutoMapper;
using Lykke.Service.PayAPI.Core;
using Lykke.Service.PayAPI.Core.Domain.PaymentRequest;

namespace Lykke.Service.PayAPI.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CreatePaymentRequest, PayInternal.Client.Models.PaymentRequest.CreatePaymentRequestModel>()
                .ForMember(dest => dest.MarkupPercent, opt => opt.MapFrom(src => src.Percent))
                .ForMember(dest => dest.MarkupPips, opt => opt.MapFrom(src => src.Pips))
                .ForMember(dest => dest.MarkupFixedFee, opt => opt.MapFrom(src => src.FixedFee))
                .ForMember(dest => dest.DueDate,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.DueDate = (DateTime) resContext.Items["DueDate"]))
                .ForMember(dest => dest.Initiator, opt => opt.UseValue(LykkePayConstants.PaymentRequestInitiator));

            CreateMap<RefundRequest, PayInternal.Client.Models.PaymentRequest.RefundRequestModel>();
        }
    }
}
