﻿using IS4.MultiArchiver.Services;
using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace IS4.MultiArchiver.Formats
{
    public class X509CertificateFormat : BinaryFileFormat<X509Certificate2>
    {
        static readonly ConditionalWeakTable<X509Certificate2, ValueType> storedTypes = new ConditionalWeakTable<X509Certificate2, ValueType>();

        public X509CertificateFormat() : base(0, null, null)
        {

        }

        public override bool CheckHeader(Span<byte> header, bool isBinary, IEncodingDetector encodingDetector)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(header.Length);
            try{
                header.CopyTo(buffer);
                var type = X509Certificate2.GetCertContentType(buffer);
                return type != X509ContentType.Unknown && type != X509ContentType.Authenticode;
            }catch{
                return true;
            }finally{
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public override TResult Match<TResult, TArgs>(Stream stream, MatchContext context, ResultFactory<X509Certificate2, TResult, TArgs> resultFactory, TArgs args)
        {
            if(stream is MemoryStream memoryStream)
            {
                var data = memoryStream.ToArray();
                var cert = new X509Certificate2(data);
                storedTypes.Add(cert, X509Certificate2.GetCertContentType(data));
                return resultFactory(cert, args);
            }else if(stream is FileStream fileStream)
            {
                var type = X509Certificate2.GetCertContentType(fileStream.Name);
                if(type == X509ContentType.Authenticode) return default;
                var cert = new X509Certificate2(fileStream.Name);
                storedTypes.Add(cert, type);
                return resultFactory(cert, args);
            }
            using(var buffer = new MemoryStream())
            {
                stream.CopyTo(buffer);
                return Match(buffer, context, resultFactory, args);
            }
        }

        private X509ContentType GetContentType(X509Certificate2 certificate)
        {
            return storedTypes.TryGetValue(certificate, out var type) ? (X509ContentType)type : X509ContentType.Unknown;
        }

        public override string GetMediaType(X509Certificate2 certificate)
        {
            switch(GetContentType(certificate))
            {
                case X509ContentType.Cert:
                    return "application/x-x509-ca-cert";
                case X509ContentType.Pfx:
                    return "application/x-pkcs12";
                case X509ContentType.Pkcs7:
                    return "application/pkcs7-mime";
            }
            return base.GetMediaType(certificate);
        }

        public override string GetExtension(X509Certificate2 certificate)
        {
            switch(GetContentType(certificate))
            {
                case X509ContentType.Cert:
                    return "der";
                case X509ContentType.Pfx:
                    return "pfx";
                case X509ContentType.Pkcs7:
                    return "p7c";
            }
            return base.GetExtension(certificate);
        }
    }
}