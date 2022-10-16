using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace F76CIG
{
    [DataContract]
    public abstract class BaseMaterialFile
    {
        private readonly uint _Signature;

        #region Fields
        protected virtual void SetDefaults()
        {
            Version = 20;
            TileU = true;
            TileV = true;
            UScale = 1.0f;
            VScale = 1.0f;
            Alpha = 1.0f;
            AlphaTestRef = 128;
            ZBufferWrite = true;
            ZBufferTest = true;
            EnvironmentMappingMaskScale = 1.0f;
            MaskWrites = MaskWriteFlags.ALBEDO | MaskWriteFlags.NORMAL | MaskWriteFlags.SPECULAR | MaskWriteFlags.AMBIENT_OCCLUSION | MaskWriteFlags.EMISSIVE | MaskWriteFlags.GLOSS;
        }
        #endregion

        protected BaseMaterialFile(uint signature)
        {
            _Signature = signature;
            SetDefaults();
        }

        #region Properties
        public uint Version { get; set; }

        [DataMember(Name = "bTileU")]
        public bool TileU { get; set; }

        [DataMember(Name = "bTileV")]
        public bool TileV { get; set; }

        [DataMember(Name = "fUOffset")]
        public float UOffset { get; set; }

        [DataMember(Name = "fVOffset")]
        public float VOffset { get; set; }

        [DataMember(Name = "fUScale")]
        public float UScale { get; set; }

        [DataMember(Name = "fVScale")]
        public float VScale { get; set; }

        [DataMember(Name = "fAlpha")]
        public float Alpha { get; set; }

        public AlphaBlendModeType AlphaBlendMode { get; set; }

        [DataMember(Name = "eAlphaBlendMode")]
        string AlphaBlendModeString
        {
            get { return AlphaBlendMode.ToString(); }
            set
            {
                AlphaBlendModeType mode;
                AlphaBlendMode = Enum.TryParse(value, true, out mode) ? mode : AlphaBlendModeType.None;
            }
        }

        [DataMember(Name = "fAlphaTestRef")]
        public byte AlphaTestRef { get; set; }

        [DataMember(Name = "bAlphaTest")]
        public bool AlphaTest { get; set; }

        [DataMember(Name = "bZBufferWrite")]
        public bool ZBufferWrite { get; set; }

        [DataMember(Name = "bZBufferTest")]
        public bool ZBufferTest { get; set; }

        [DataMember(Name = "bScreenSpaceReflections")]
        public bool ScreenSpaceReflections { get; set; }

        [DataMember(Name = "bWetnessControl_ScreenSpaceReflections")]
        public bool WetnessControlScreenSpaceReflections { get; set; }

        [DataMember(Name = "bDecal")]
        public bool Decal { get; set; }

        [DataMember(Name = "bTwoSided")]
        public bool TwoSided { get; set; }

        [DataMember(Name = "bDecalNoFade")]
        public bool DecalNoFade { get; set; }

        [DataMember(Name = "bNonOccluder")]
        public bool NonOccluder { get; set; }

        [DataMember(Name = "bRefraction")]
        public bool Refraction { get; set; }

        [DataMember(Name = "fRefractionFalloff")]
        public bool RefractionFalloff { get; set; }

        [DataMember(Name = "fRefractionPower")]
        public float RefractionPower { get; set; }

        [DataMember(Name = "bEnvironmentMapping")]
        public bool EnvironmentMapping { get; set; }

        [DataMember(Name = "fEnvironmentMappingMaskScale")]
        public float EnvironmentMappingMaskScale { get; set; }

        [DataMember(Name = "bDepthBias")]
        public bool DepthBias { get; set; }

        [DataMember(Name = "bGrayscaleToPaletteColor")]
        public bool GrayscaleToPaletteColor { get; set; }

        [DataMember(Name = "bWriteMaskAlbedo")]
        bool WriteMaskAlbedo
        {
            get { return MaskWrites.HasFlag(MaskWriteFlags.ALBEDO); }
            set { MaskWrites |= MaskWriteFlags.ALBEDO; }
        }

        [DataMember(Name = "bWriteMaskNormal")]
        bool WriteMaskNormal
        {
            get { return MaskWrites.HasFlag(MaskWriteFlags.NORMAL); }
            set { MaskWrites |= MaskWriteFlags.NORMAL; }
        }

        [DataMember(Name = "bWriteMaskSpecular")]
        bool WriteMaskSpecular
        {
            get { return MaskWrites.HasFlag(MaskWriteFlags.SPECULAR); }
            set { MaskWrites |= MaskWriteFlags.SPECULAR; }
        }

        [DataMember(Name = "bWriteMaskAmbientOcclusion")]
        bool WriteMaskAmbientOcclusion
        {
            get { return MaskWrites.HasFlag(MaskWriteFlags.AMBIENT_OCCLUSION); }
            set { MaskWrites |= MaskWriteFlags.AMBIENT_OCCLUSION; }
        }

        [DataMember(Name = "bWriteMaskEmissive")]
        bool WriteMaskEmissive
        {
            get { return MaskWrites.HasFlag(MaskWriteFlags.EMISSIVE); }
            set { MaskWrites |= MaskWriteFlags.EMISSIVE; }
        }

        [DataMember(Name = "bWriteMaskGloss")]
        bool WriteMaskGloss
        {
            get { return MaskWrites.HasFlag(MaskWriteFlags.GLOSS); }
            set { MaskWrites |= MaskWriteFlags.GLOSS; }
        }

        public MaskWriteFlags MaskWrites { get; set; }
        #endregion

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            SetDefaults();
        }

        public virtual void Deserialize(BinaryReader input)
        {
            var magic = input.ReadUInt32();
            if (magic != _Signature)
            {
                throw new FormatException();
            }

            Version = input.ReadUInt32();

            var tileFlags = input.ReadUInt32();
            TileU = (tileFlags & 2) != 0;
            TileV = (tileFlags & 1) != 0;
            UOffset = input.ReadSingle();
            VOffset = input.ReadSingle();
            UScale = input.ReadSingle();
            VScale = input.ReadSingle();

            Alpha = input.ReadSingle();
            var alphaBlendMode0 = input.ReadByte();
            var alphaBlendMode1 = input.ReadUInt32();
            var alphaBlendMode2 = input.ReadUInt32();
            AlphaBlendMode = ConvertAlphaBlendMode(alphaBlendMode0, alphaBlendMode1, alphaBlendMode2);
            AlphaTestRef = input.ReadByte();
            AlphaTest = input.ReadBoolean();

            ZBufferWrite = input.ReadBoolean();
            ZBufferTest = input.ReadBoolean();
            ScreenSpaceReflections = input.ReadBoolean();
            WetnessControlScreenSpaceReflections = input.ReadBoolean();
            Decal = input.ReadBoolean();
            TwoSided = input.ReadBoolean();
            DecalNoFade = input.ReadBoolean();
            NonOccluder = input.ReadBoolean();

            Refraction = input.ReadBoolean();
            RefractionFalloff = input.ReadBoolean();
            RefractionPower = input.ReadSingle();

            if (Version < 10)
            {
                EnvironmentMapping = input.ReadBoolean();
                EnvironmentMappingMaskScale = input.ReadSingle();
            }
            else
            {
                DepthBias = input.ReadBoolean();
            }

            GrayscaleToPaletteColor = input.ReadBoolean();

            if (Version >= 6)
            {
                MaskWrites = (MaskWriteFlags)input.ReadByte();
            }
        }

        public virtual void Serialize(BinaryWriter output)
        {
            output.Write(_Signature);
            output.Write(Version);

            uint tileFlags = 0;
            if (TileU) tileFlags += 2;
            if (TileV) tileFlags += 1;
            output.Write(tileFlags);

            output.Write(UOffset);
            output.Write(VOffset);
            output.Write(UScale);
            output.Write(VScale);

            output.Write(Alpha);

            byte alphaBlendMode0 = 0;
            uint alphaBlendMode1 = 0;
            uint alphaBlendMode2 = 0;
            ConvertAlphaBlendMode(AlphaBlendMode, ref alphaBlendMode0, ref alphaBlendMode1, ref alphaBlendMode2);
            output.Write(alphaBlendMode0);
            output.Write(alphaBlendMode1);
            output.Write(alphaBlendMode2);

            output.Write(AlphaTestRef);
            output.Write(AlphaTest);

            output.Write(ZBufferWrite);
            output.Write(ZBufferTest);
            output.Write(ScreenSpaceReflections);
            output.Write(WetnessControlScreenSpaceReflections);
            output.Write(Decal);
            output.Write(TwoSided);
            output.Write(DecalNoFade);
            output.Write(NonOccluder);

            output.Write(Refraction);
            output.Write(RefractionFalloff);
            output.Write(RefractionPower);

            if (Version < 10)
            {
                output.Write(EnvironmentMapping);
                output.Write(EnvironmentMappingMaskScale);
            }
            else
            {
                output.Write(DepthBias);
            }

            output.Write(GrayscaleToPaletteColor);

            if (Version >= 6)
            {
                output.Write((byte)MaskWrites);
            }
        }

        public bool Open(FileStream file)
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(file))
                {
                    Deserialize(reader);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool Save(FileStream file)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(file))
                {
                    Serialize(writer);
                    writer.Flush();
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        protected static string ReadString(BinaryReader input)
        {
            var length = input.ReadUInt32();
            string str = new string(input.ReadChars((int)length));

            int index = str.LastIndexOf('\0');
            if (index >= 0)
                str = str.Remove(index, 1);

            return str;
        }

        protected static void WriteString(BinaryWriter output, string str)
        {
            var length = str.Length + 1;
            output.Write(length);
            output.Write((str + "\0").ToCharArray());
        }

        public enum AlphaBlendModeType
        {
            Unknown = 0,
            None,
            Standard,
            Additive,
            Multiplicative,
        }

        private static AlphaBlendModeType ConvertAlphaBlendMode(byte a, uint b, uint c)
        {
            if (a == 0 && b == 6 && c == 7)
            {
                return AlphaBlendModeType.Unknown;
            }
            else if (a == 0 && b == 0 && c == 0)
            {
                return AlphaBlendModeType.None;
            }
            else if (a == 1 && b == 6 && c == 7)
            {
                return AlphaBlendModeType.Standard;
            }
            else if (a == 1 && b == 6 && c == 0)
            {
                return AlphaBlendModeType.Additive;
            }
            else if (a == 1 && b == 4 && c == 1)
            {
                return AlphaBlendModeType.Multiplicative;
            }

            throw new NotSupportedException();
        }

        private static void ConvertAlphaBlendMode(AlphaBlendModeType type, ref byte a, ref uint b, ref uint c)
        {
            if (type == AlphaBlendModeType.Unknown)
            {
                a = 0;
                b = 6;
                c = 7;
            }
            else if (type == AlphaBlendModeType.None)
            {
                a = 0;
                b = 0;
                c = 0;
            }
            else if (type == AlphaBlendModeType.Standard)
            {
                a = 1;
                b = 6;
                c = 7;
            }
            else if (type == AlphaBlendModeType.Additive)
            {
                a = 1;
                b = 6;
                c = 0;
            }
            else if (type == AlphaBlendModeType.Multiplicative)
            {
                a = 1;
                b = 4;
                c = 1;
            }
            else
                throw new NotSupportedException();
        }

        protected struct Color
        {
            public readonly float R;
            public readonly float G;
            public readonly float B;

            public Color(float r, float g, float b)
            {
                R = r;
                G = g;
                B = b;
            }

            public uint ToUInt32()
            {
                uint value = 0;
                value |= (byte)(R * 255);
                value <<= 8;
                value |= (byte)(G * 255);
                value <<= 8;
                value |= (byte)(B * 255);
                return value;
            }

            public static Color FromUInt32(uint value)
            {
                const float multiplier = 1.0f / 255;
                var b = (byte)(value & 0xFF);
                value >>= 8;
                var g = (byte)(value & 0xFF);
                value >>= 8;
                var r = (byte)(value & 0xFF);
                return new Color(r * multiplier, g * multiplier, b * multiplier);
            }

            public string ToHexString()
            {
                return string.Format("#{0:x6}", ToUInt32() & 0xFFFFFFu);
            }

            public static Color FromHexString(string str)
            {
                var text = str.ToLowerInvariant();
                if (text.StartsWith("#") == true)
                    text = text.Substring(1);

                if (text == "000")
                    return FromUInt32(0x000000u);

                if (text == "fff")
                    return FromUInt32(0xFFFFFFu);

                if (text.Length == 3)
                {
                    uint val = uint.Parse(text, NumberStyles.AllowHexSpecifier);
                    val = ((val & 0xF00) << 8) | ((val & 0x0F0) << 4) | ((val & 0x00F) << 0);
                    val |= val << 4;
                    return FromUInt32(val);
                }

                if (text.Length == 6)
                    return FromUInt32(uint.Parse(text, NumberStyles.AllowHexSpecifier));

                return new Color(1.0f, 1.0f, 1.0f);
            }

            public static Color Read(BinaryReader input)
            {
                var r = input.ReadSingle();
                var g = input.ReadSingle();
                var b = input.ReadSingle();
                return new Color(r, g, b);
            }

            public void Write(BinaryWriter output)
            {
                output.Write(R);
                output.Write(G);
                output.Write(B);
            }
        }

        public enum MaskWriteFlags
        {
            ALBEDO = 1,
            NORMAL = 2,
            SPECULAR = 4,
            AMBIENT_OCCLUSION = 8,
            EMISSIVE = 16,
            GLOSS = 32
        }
    }


    [DataContract]
    public class BGSM : BaseMaterialFile
    {
        public const uint Signature = 0x4D534742u;

        #region Fields
        protected override void SetDefaults()
        {
            base.SetDefaults();

            DiffuseTexture = "";
            NormalTexture = "";
            SmoothSpecTexture = "";
            GreyscaleTexture = "";
            EnvmapTexture = "";
            GlowTexture = "";
            InnerLayerTexture = "";
            WrinklesTexture = "";
            DisplacementTexture = "";
            SpecularTexture = "";
            LightingTexture = "";
            FlowTexture = "";
            DistanceFieldAlphaTexture = "";
            RimPower = 2.0f;
            SubsurfaceLightingRolloff = 0.3f;
            SpecularColor = 0xFFFFFFFFu;
            SpecularMult = 1.0f;
            Smoothness = 1.0f;
            FresnelPower = 5.0f;
            WetnessControlSpecScale = -1.0f;
            WetnessControlSpecPowerScale = -1.0f;
            WetnessControlSpecMinvar = -1.0f;
            WetnessControlEnvMapScale = -1.0f;
            WetnessControlFresnelPower = -1.0f;
            WetnessControlMetalness = -1.0f;
            RootMaterialPath = "";
            EmittanceColor = 0xFFFFFFFFu;
            EmittanceMult = 1.0f;
            HairTintColor = 0x808080u;
            DisplacementTextureBias = -0.5f;
            DisplacementTextureScale = 10.0f;
            TessellationPnScale = 1.0f;
            TessellationBaseFactor = 1.0f;
            GrayscaleToPaletteScale = 1.0f;
        }
        #endregion

        public BGSM()
            : base(Signature)
        {
        }

        #region Properties
        [DataMember(Name = "sDiffuseTexture")]
        public string DiffuseTexture { get; set; }

        [DataMember(Name = "sNormalTexture")]
        public string NormalTexture { get; set; }

        [DataMember(Name = "sSmoothSpecTexture")]
        public string SmoothSpecTexture { get; set; }

        [DataMember(Name = "sGreyscaleTexture")]
        public string GreyscaleTexture { get; set; }

        [DataMember(Name = "sEnvmapTexture")]
        public string EnvmapTexture { get; set; }

        [DataMember(Name = "sGlowTexture")]
        public string GlowTexture { get; set; }

        [DataMember(Name = "sInnerLayerTexture")]
        public string InnerLayerTexture { get; set; }

        [DataMember(Name = "sWrinklesTexture")]
        public string WrinklesTexture { get; set; }

        [DataMember(Name = "sDisplacementTexture")]
        public string DisplacementTexture { get; set; }

        [DataMember(Name = "sSpecularTexture")]
        public string SpecularTexture { get; set; }

        [DataMember(Name = "sLightingTexture")]
        public string LightingTexture { get; set; }

        [DataMember(Name = "sFlowTexture")]
        public string FlowTexture { get; set; }

        [DataMember(Name = "sDistanceFieldAlphaTexture")]
        public string DistanceFieldAlphaTexture { get; set; }

        [DataMember(Name = "bEnableEditorAlphaRef")]
        public bool EnableEditorAlphaRef { get; set; }

        [DataMember(Name = "bRimLighting")]
        public bool RimLighting { get; set; }

        [DataMember(Name = "fRimPower")]
        public float RimPower { get; set; }

        [DataMember(Name = "fBackLightPower")]
        public float BackLightPower { get; set; }

        [DataMember(Name = "bSubsurfaceLighting")]
        public bool SubsurfaceLighting { get; set; }

        [DataMember(Name = "fSubsurfaceLightingRolloff")]
        public float SubsurfaceLightingRolloff { get; set; }

        [DataMember(Name = "bSpecularEnabled")]
        public bool SpecularEnabled { get; set; }

        public uint SpecularColor { get; set; }

        [DataMember(Name = "cSpecularColor")]
        string SpecularColorString
        {
            get { return Color.FromUInt32(SpecularColor).ToHexString(); }
            set { SpecularColor = Color.FromHexString(value).ToUInt32(); }
        }

        [DataMember(Name = "fSpecularMult")]
        public float SpecularMult { get; set; }

        [DataMember(Name = "fSmoothness")]
        public float Smoothness { get; set; }

        [DataMember(Name = "fFresnelPower")]
        public float FresnelPower { get; set; }

        [DataMember(Name = "fWetnessControl_SpecScale")]
        public float WetnessControlSpecScale { get; set; }

        [DataMember(Name = "fWetnessControl_SpecPowerScale")]
        public float WetnessControlSpecPowerScale { get; set; }

        [DataMember(Name = "fWetnessControl_SpecMinvar")]
        public float WetnessControlSpecMinvar { get; set; }

        [DataMember(Name = "fWetnessControl_EnvMapScale")]
        public float WetnessControlEnvMapScale { get; set; }

        [DataMember(Name = "fWetnessControl_FresnelPower")]
        public float WetnessControlFresnelPower { get; set; }

        [DataMember(Name = "fWetnessControl_Metalness")]
        public float WetnessControlMetalness { get; set; }

        [DataMember(Name = "sRootMaterialPath")]
        public string RootMaterialPath { get; set; }

        [DataMember(Name = "bAnisoLighting")]
        public bool AnisoLighting { get; set; }

        [DataMember(Name = "bEmitEnabled")]
        public bool EmitEnabled { get; set; }

        public uint EmittanceColor { get; set; }

        [DataMember(Name = "cEmittanceColor")]
        string EmittanceColorString
        {
            get { return Color.FromUInt32(EmittanceColor).ToHexString(); }
            set { EmittanceColor = Color.FromHexString(value).ToUInt32(); }
        }

        [DataMember(Name = "fEmittanceMult")]
        public float EmittanceMult { get; set; }

        [DataMember(Name = "bModelSpaceNormals")]
        public bool ModelSpaceNormals { get; set; }

        [DataMember(Name = "bExternalEmittance")]
        public bool ExternalEmittance { get; set; }

        [DataMember(Name = "bBackLighting")]
        public bool BackLighting { get; set; }

        [DataMember(Name = "bReceiveShadows")]
        public bool ReceiveShadows { get; set; }

        [DataMember(Name = "bHideSecret")]
        public bool HideSecret { get; set; }

        [DataMember(Name = "bCastShadows")]
        public bool CastShadows { get; set; }

        [DataMember(Name = "bDissolveFade")]
        public bool DissolveFade { get; set; }

        [DataMember(Name = "bAssumeShadowmask")]
        public bool AssumeShadowmask { get; set; }

        [DataMember(Name = "bGlowmap")]
        public bool Glowmap { get; set; }

        [DataMember(Name = "bEnvironmentMappingWindow")]
        public bool EnvironmentMappingWindow { get; set; }

        [DataMember(Name = "bEnvironmentMappingEye")]
        public bool EnvironmentMappingEye { get; set; }

        [DataMember(Name = "bHair")]
        public bool Hair { get; set; }

        public uint HairTintColor { get; set; }

        [DataMember(Name = "cHairTintColor")]
        string HairTintColorString
        {
            get { return Color.FromUInt32(HairTintColor).ToHexString(); }
            set { HairTintColor = Color.FromHexString(value).ToUInt32(); }
        }

        [DataMember(Name = "bTree")]
        public bool Tree { get; set; }

        [DataMember(Name = "bFacegen")]
        public bool Facegen { get; set; }

        [DataMember(Name = "bSkinTint")]
        public bool SkinTint { get; set; }

        [DataMember(Name = "bTessellate")]
        public bool Tessellate { get; set; }

        [DataMember(Name = "fDisplacementTextureBias")]
        public float DisplacementTextureBias { get; set; }

        [DataMember(Name = "fDisplacementTextureScale")]
        public float DisplacementTextureScale { get; set; }

        [DataMember(Name = "fTessellationPnScale")]
        public float TessellationPnScale { get; set; }

        [DataMember(Name = "fTessellationBaseFactor")]
        public float TessellationBaseFactor { get; set; }

        [DataMember(Name = "fTessellationFadeDistance")]
        public float TessellationFadeDistance { get; set; }

        [DataMember(Name = "fGrayscaleToPaletteScale")]
        public float GrayscaleToPaletteScale { get; set; }

        [DataMember(Name = "bSkewSpecularAlpha")]
        public bool SkewSpecularAlpha { get; set; }

        [DataMember(Name = "bTranslucency")]
        public bool Translucency { get; set; }

        public uint TranslucencySubsurfaceColor { get; set; }

        [DataMember(Name = "cTranslucencySubsurfaceColor")]
        string TranslucencySubsurfaceColorString
        {
            get { return Color.FromUInt32(TranslucencySubsurfaceColor).ToHexString(); }
            set { TranslucencySubsurfaceColor = Color.FromHexString(value).ToUInt32(); }
        }

        [DataMember(Name = "fTranslucencyTransmissiveScale")]
        public float TranslucencyTransmissiveScale { get; set; }

        [DataMember(Name = "fTranslucencyTurbulence")]
        public float TranslucencyTurbulence { get; set; }

        [DataMember(Name = "bPBR")]
        public bool PBR { get; set; }

        [DataMember(Name = "bCustomPorosity")]
        public bool CustomPorosity { get; set; }

        [DataMember(Name = "fPorosityValue")]
        public float PorosityValue { get; set; }

        [DataMember(Name = "fLumEmittance")]
        public float LumEmittance { get; set; }

        [DataMember(Name = "bTranslucencyThickObject")]
        public bool TranslucencyThickObject { get; set; }

        [DataMember(Name = "bTranslucencyMixAlbedoWithSubsurfaceColor")]
        public bool TranslucencyMixAlbedoWithSubsurfaceColor { get; set; }

        [DataMember(Name = "bUseAdaptativeEmissive")]
        public bool UseAdaptativeEmissive { get; set; }

        [DataMember(Name = "fAdaptativeEmissive_ExposureOffset")]
        public float AdaptativeEmissive_ExposureOffset { get; set; }

        [DataMember(Name = "fAdaptativeEmissive_FinalExposureMin")]
        public float AdaptativeEmissive_FinalExposureMin { get; set; }

        [DataMember(Name = "fAdaptativeEmissive_FinalExposureMax")]
        public float AdaptativeEmissive_FinalExposureMax { get; set; }

        [DataMember(Name = "bTerrain")]
        public bool Terrain { get; set; }

        [DataMember(Name = "fTerrainThresholdFalloff")]
        public float TerrainThresholdFalloff { get; set; }

        [DataMember(Name = "fTerrainTilingDistance")]
        public float TerrainTilingDistance { get; set; }

        [DataMember(Name = "fTerrainRotationAngle")]
        public float TerrainRotationAngle { get; set; }

        [DataMember]
        public uint UnkInt1 { get; set; }
        #endregion

        public override void Deserialize(BinaryReader input)
        {
            base.Deserialize(input);

            DiffuseTexture = ReadString(input);
            NormalTexture = ReadString(input);
            SmoothSpecTexture = ReadString(input);
            GreyscaleTexture = ReadString(input);

            if (Version > 2)
            {
                GlowTexture = ReadString(input);
                WrinklesTexture = ReadString(input);
                SpecularTexture = ReadString(input);
                LightingTexture = ReadString(input);
                FlowTexture = ReadString(input);

                if (Version >= 17)
                {
                    DistanceFieldAlphaTexture = ReadString(input);
                }
            }
            else
            {
                EnvmapTexture = ReadString(input);
                GlowTexture = ReadString(input);
                InnerLayerTexture = ReadString(input);
                WrinklesTexture = ReadString(input);
                DisplacementTexture = ReadString(input);
            }

            EnableEditorAlphaRef = input.ReadBoolean();

            if (Version >= 8)
            {
                Translucency = input.ReadBoolean();
                TranslucencyThickObject = input.ReadBoolean();
                TranslucencyMixAlbedoWithSubsurfaceColor = input.ReadBoolean();
                TranslucencySubsurfaceColor = Color.Read(input).ToUInt32();
                TranslucencyTransmissiveScale = input.ReadSingle();
                TranslucencyTurbulence = input.ReadSingle();
            }
            else
            {
                RimLighting = input.ReadBoolean();
                RimPower = input.ReadSingle();
                BackLightPower = input.ReadSingle();

                SubsurfaceLighting = input.ReadBoolean();
                SubsurfaceLightingRolloff = input.ReadSingle();
            }

            SpecularEnabled = input.ReadBoolean();
            SpecularColor = Color.Read(input).ToUInt32();
            SpecularMult = input.ReadSingle();
            Smoothness = input.ReadSingle();

            FresnelPower = input.ReadSingle();
            WetnessControlSpecScale = input.ReadSingle();
            WetnessControlSpecPowerScale = input.ReadSingle();
            WetnessControlSpecMinvar = input.ReadSingle();

            if (Version < 10)
            {
                WetnessControlEnvMapScale = input.ReadSingle();
            }

            WetnessControlFresnelPower = input.ReadSingle();
            WetnessControlMetalness = input.ReadSingle();

            if (Version > 2)
            {
                PBR = input.ReadBoolean();

                if (Version >= 9)
                {
                    CustomPorosity = input.ReadBoolean();
                    PorosityValue = input.ReadSingle();
                }
            }

            RootMaterialPath = ReadString(input);

            AnisoLighting = input.ReadBoolean();
            EmitEnabled = input.ReadBoolean();

            if (EmitEnabled)
            {
                EmittanceColor = Color.Read(input).ToUInt32();
            }

            EmittanceMult = input.ReadSingle();
            ModelSpaceNormals = input.ReadBoolean();
            ExternalEmittance = input.ReadBoolean();

            if (Version >= 12)
            {
                LumEmittance = input.ReadSingle();
            }

            if (Version >= 13)
            {
                UseAdaptativeEmissive = input.ReadBoolean();
                AdaptativeEmissive_ExposureOffset = input.ReadSingle();
                AdaptativeEmissive_FinalExposureMin = input.ReadSingle();
                AdaptativeEmissive_FinalExposureMax = input.ReadSingle();
            }

            if (Version < 8)
            {
                BackLighting = input.ReadBoolean();
            }

            ReceiveShadows = input.ReadBoolean();
            HideSecret = input.ReadBoolean();
            CastShadows = input.ReadBoolean();
            DissolveFade = input.ReadBoolean();
            AssumeShadowmask = input.ReadBoolean();

            Glowmap = input.ReadBoolean();

            if (Version < 7)
            {
                EnvironmentMappingWindow = input.ReadBoolean();
                EnvironmentMappingEye = input.ReadBoolean();
            }

            Hair = input.ReadBoolean();
            HairTintColor = Color.Read(input).ToUInt32();

            Tree = input.ReadBoolean();
            Facegen = input.ReadBoolean();
            SkinTint = input.ReadBoolean();
            Tessellate = input.ReadBoolean();

            if (Version < 3)
            {
                DisplacementTextureBias = input.ReadSingle();
                DisplacementTextureScale = input.ReadSingle();
                TessellationPnScale = input.ReadSingle();
                TessellationBaseFactor = input.ReadSingle();
                TessellationFadeDistance = input.ReadSingle();
            }

            GrayscaleToPaletteScale = input.ReadSingle();

            if (Version >= 1)
            {
                SkewSpecularAlpha = input.ReadBoolean();
            }

            if (Version >= 3)
            {
                Terrain = input.ReadBoolean();

                if (Terrain)
                {
                    if (Version == 3)
                    {
                        UnkInt1 = input.ReadUInt32();
                    }

                    TerrainThresholdFalloff = input.ReadSingle();
                    TerrainTilingDistance = input.ReadSingle();
                    TerrainRotationAngle = input.ReadSingle();
                }
            }
        }

        public override void Serialize(BinaryWriter output)
        {
            base.Serialize(output);

            WriteString(output, DiffuseTexture);
            WriteString(output, NormalTexture);
            WriteString(output, SmoothSpecTexture);
            WriteString(output, GreyscaleTexture);

            if (Version > 2)
            {
                WriteString(output, GlowTexture);
                WriteString(output, WrinklesTexture);
                WriteString(output, SpecularTexture);
                WriteString(output, LightingTexture);
                WriteString(output, FlowTexture);

                if (Version >= 17)
                {
                    WriteString(output, DistanceFieldAlphaTexture);
                }
            }
            else
            {
                WriteString(output, EnvmapTexture);
                WriteString(output, GlowTexture);
                WriteString(output, InnerLayerTexture);
                WriteString(output, WrinklesTexture);
                WriteString(output, DisplacementTexture);
            }

            output.Write(EnableEditorAlphaRef);

            if (Version >= 8)
            {
                output.Write(Translucency);
                output.Write(TranslucencyThickObject);
                output.Write(TranslucencyMixAlbedoWithSubsurfaceColor);
                Color.FromUInt32(TranslucencySubsurfaceColor).Write(output);
                output.Write(TranslucencyTransmissiveScale);
                output.Write(TranslucencyTurbulence);
            }
            else
            {
                output.Write(RimLighting);
                output.Write(RimPower);
                output.Write(BackLightPower);

                output.Write(SubsurfaceLighting);
                output.Write(SubsurfaceLightingRolloff);
            }

            output.Write(SpecularEnabled);
            Color.FromUInt32(SpecularColor).Write(output);
            output.Write(SpecularMult);
            output.Write(Smoothness);

            output.Write(FresnelPower);
            output.Write(WetnessControlSpecScale);
            output.Write(WetnessControlSpecPowerScale);
            output.Write(WetnessControlSpecMinvar);

            if (Version < 10)
            {
                output.Write(WetnessControlEnvMapScale);
            }

            output.Write(WetnessControlFresnelPower);
            output.Write(WetnessControlMetalness);

            if (Version > 2)
            {
                output.Write(PBR);

                if (Version >= 9)
                {
                    output.Write(CustomPorosity);
                    output.Write(PorosityValue);
                }
            }

            WriteString(output, RootMaterialPath);

            output.Write(AnisoLighting);
            output.Write(EmitEnabled);

            if (EmitEnabled)
            {
                Color.FromUInt32(EmittanceColor).Write(output);
            }

            output.Write(EmittanceMult);
            output.Write(ModelSpaceNormals);
            output.Write(ExternalEmittance);

            if (Version >= 12)
            {
                output.Write(LumEmittance);
            }

            if (Version >= 13)
            {
                output.Write(UseAdaptativeEmissive);
                output.Write(AdaptativeEmissive_ExposureOffset);
                output.Write(AdaptativeEmissive_FinalExposureMin);
                output.Write(AdaptativeEmissive_FinalExposureMax);
            }

            if (Version < 8)
            {
                output.Write(BackLighting);
            }

            output.Write(ReceiveShadows);
            output.Write(HideSecret);
            output.Write(CastShadows);
            output.Write(DissolveFade);
            output.Write(AssumeShadowmask);

            output.Write(Glowmap);

            if (Version < 7)
            {
                output.Write(EnvironmentMappingWindow);
                output.Write(EnvironmentMappingEye);
            }

            output.Write(Hair);
            Color.FromUInt32(HairTintColor).Write(output);

            output.Write(Tree);
            output.Write(Facegen);
            output.Write(SkinTint);
            output.Write(Tessellate);

            if (Version < 3)
            {
                output.Write(DisplacementTextureBias);
                output.Write(DisplacementTextureScale);
                output.Write(TessellationPnScale);
                output.Write(TessellationBaseFactor);
                output.Write(TessellationFadeDistance);
            }

            output.Write(GrayscaleToPaletteScale);

            if (Version >= 1)
            {
                output.Write(SkewSpecularAlpha);
            }

            if (Version >= 3)
            {
                output.Write(Terrain);

                if (Terrain)
                {
                    if (Version == 3)
                    {
                        output.Write(UnkInt1);
                    }

                    output.Write(TerrainThresholdFalloff);
                    output.Write(TerrainTilingDistance);
                    output.Write(TerrainRotationAngle);
                }
            }
        }
    }
}