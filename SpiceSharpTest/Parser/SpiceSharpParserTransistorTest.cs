using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.Components;

namespace SpiceSharpTest.Parser
{
    [TestClass]
    public class SpiceSharpParserTransistorTest : Framework
    {
        [TestMethod]
        public void MOS1Test()
        {
            var netlist = Run(
                ".model lvl1mod nmos(level = 1 vto = 0.5 kp = 1 gamma = 2 phi = 3 lambda = 4 rd = 5",
                "+ rs = 6 cbd = 7 cbs = 8 is = 9 pb = 10 cgso = 11 cgdo = 12 cgbo = 13 rsh = 14",
                "+ cj = 15 mj = 16 cjsw = 17 mjsw = 18 js = 19 tox = 20 nsub = 21 nss = 22",
                "+ tpg = 24 ld = 26 uo = 27",
                "+ kf = 33 af = 34 fc = 35 tnom = 40)",
                "M1 d g s b lvl1mod l=100u w=200u"
                );
            Test<MOS1Model>(netlist, "lvl1mod", new string[] {
                "vto", "kp", "gamma", "phi", "lambda", "rd",
                "rs", "cbd", "cbs", "is", "pb", "cgso", "cgdo", "cgbo", "rsh",
                "cj", "mj", "cjsw", "mjsw", "js", "tox", "nsub", "nss", 
                "tpg", "ld", "uo", 
                "kf", "af", "fc", "tnom"
            }, new double[] {
                0.5, 1, 2, 3, 4, 5,
                6, 7, 8, 9, 10, 11, 12, 13, 14,
                15, 16, 17, 18, 19, 20, 21, 22, 
                24, 26, 27,
                33, 34, 35, 40
            });
            Test<MOS1>(netlist, "M1", new string[] { "w", "l" }, new double[] { 200e-6, 100e-6 }, new string[] { "d", "g", "s", "b" });
        }

        [TestMethod]
        public void MOS2Test()
        {
            var netlist = Run(".model lvl2mod pmos level = 2 tnom = 0 vto = 1",
                "+ kp = 2 gamma = 3 phi = 4 lambda = 5 rd = 6 rs = 7 cbd = 8",
                "+ cbs = 9 is = 10 pb = 11 cgso = 12 cgdo = 13 cgbo = 14 cj = 15",
                "+ mj = 16 cjsw = 17 mjsw = 18 js = 19 tox = 20 ld = 21 rsh = 22",
                "+ u0 = 23 fc = 24 nsub = 25 tpg = 26 nss = 27 nfs = 28 delta = 29",
                "+ uexp = 30 vmax = 31 xj = 32 neff = 33 ucrit = 34 kf = 35 af = 36",
                "M1 drain gate source bulk lvl2mod w = 5u l = 1u");
            Test<MOS2Model>(netlist, "lvl2mod", new string[]
            {
                "tnom", "vto",
                "kp", "gamma", "phi", "lambda", "rd", "rs", "cbd",
                "cbs", "is", "pb", "cgso", "cgdo", "cgbo", "cj",
                "mj", "cjsw", "mjsw", "js", "tox", "ld", "rsh",
                "u0", "fc", "nsub", "tpg", "nss", "nfs", "delta",
                "uexp", "vmax", "xj", "neff", "ucrit", "kf", "af"
            }, new double[] {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
                16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28,
                29, 30, 31, 32, 33, 34, 35, 36
            });
            Test<MOS2>(netlist, "M1", new string[] { "w", "l" }, new double[] { 5e-6, 1e-6 }, new string[] { "drain", "gate", "source", "bulk" });
        }

        [TestMethod]
        public void MOS3Test()
        {
            var netlist = Run(".model lvl3mod nmos level = 3 vto = 0 kp = 1",
                "+ gamma = 2 phi = 3 rd = 4 rs = 5 cbd = 6 cbs = 7 is = 8",
                "+ pb = 9 cgso = 10 cgdo = 11 cgbo = 12 rsh = 13 cj = 14",
                "+ mj = 15 cjsw = 16 mjsw = 17 js = 18 tox = 19 ld = 20",
                "+ u0 = 21 fc = 22 nsub = 23 tpg = 24 nss = 25 eta = 26",
                "+ nfs = 27 theta = 28 vmax = 29 kappa = 30 xj = 31",
                "+ tnom = 32 kf = 33 af = 34 delta = 38",
                "M1 dr ga so bu lvl3mod w=10u l = 5u");
            Test<MOS3Model>(netlist, "lvl3mod", new string[]
            {
                "vto", "kp",
                "gamma", "phi", "rd", "rs", "cbd", "cbs", "is",
                "pb", "cgso", "cgdo", "cgbo", "rsh", "cj",
                "mj", "cjsw", "mjsw", "js", "tox", "ld",
                "u0", "fc", "nsub", "tpg", "nss", "eta",
                "nfs", "theta", "vmax", "kappa", "xj",
                "tnom", "kf", "af", "input_delta"
            }, new double[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
                16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28,
                29, 30, 31, 32, 33, 34, 38
            });
            Test<MOS3>(netlist, "M1", new string[] { "w", "l" }, new double[] { 10e-6, 5e-6 }, new string[] { "dr", "ga", "so", "bu" });
        }

        [TestMethod]
        public void BSIM1Test()
        {
            var netlist = Run(".model lvl4mod nmos level = 4 vfb = 0 lvfb = 1 wvfb = 2 phi = 3 lphi = 4",
                "+ wphi = 5 k1 = 6 lk1 = 7 wk1 = 8 k2 = 9 lk2 = 10 wk2 = 11 eta = 12 leta = 13 weta = 14",
                "+ x2e = 15 lx2e = 16 wx2e = 17 x3e = 18 lx3e = 19 wx3e = 20 dl = 21 dw = 22 muz = 23 x2mz = 24",
                "+ lx2mz = 25 wx2mz = 26 mus = 27 lmus = 28 wmus = 29 x2ms = 30 lx2ms = 31 wx2ms = 32 x3ms = 33",
                "+ lx3ms = 34 wx3ms = 35 u0 = 36 lu0 = 37 wu0 = 38 x2u0 = 39 lx2u0 = 40 wx2u0 = 41 u1 = 42",
                "+ lu1 = 43 wu1 = 44 x2u1 = 45 lx2u1 = 46 wx2u1 = 47 x3u1 = 48 lx3u1 = 49 wx3u1 = 50 n0 = 51",
                "+ ln0 = 52 wn0 = 53 nb = 54 lnb = 55 wnb = 56 nd = 57 lnd = 58 wnd = 59 tox = 60 temp = 61",
                "+ vdd = 62 cgso = 63 cgdo = 64 cgbo = 65 xpart = 66 rsh = 67 js = 68 pb = 69 mj = 70 pbsw = 71",
                "+ mjsw = 72 cj = 73 cjsw = 74 wdf = 75 dell = 76",
                "MB d g s b lvl4mod w = 10u l = 11u ad = 1e-12 as = 2e-12 nrs = 2 nrd = 3 pd = 30u ps = 40u");
            Test<BSIM1Model>(netlist, "lvl4mod", new string[]
            {
                "vfb", "lvfb", "wvfb", "phi", "lphi", "wphi", "k1", "lk1", "wk1", "k2", "lk2", "wk2", "eta", "leta",
                "weta", "x2e", "lx2e", "wx2e", "x3e", "lx3e", "wx3e", "dl", "dw", "muz", "x2mz", "lx2mz", "wx2mz",
                "mus", "lmus", "wmus", "x2ms", "lx2ms", "wx2ms", "x3ms", "lx3ms", "wx3ms", "u0", "lu0", "wu0", "x2u0",
                "lx2u0", "wx2u0", "u1", "lu1", "wu1", "x2u1", "lx2u1", "wx2u1", "x3u1", "lx3u1", "wx3u1", "n0", "ln0",
                "wn0", "nb", "lnb", "wnb", "nd", "lnd", "wnd", "tox", "temp", "vdd", "cgso", "cgdo", "cgbo", "xpart",
                "rsh", "js", "pb", "mj", "pbsw", "mjsw", "cj", "cjsw", "wdf", "dell"
            }, new double[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
                26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
                50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76
            });
            Test<BSIM1>(netlist, "MB",
                new string[] { "w", "l", "ad", "as", "nrs", "nrd", "pd", "ps" },
                new double[] { 10e-6, 11e-6, 1e-12, 2e-12, 2.0, 3.0, 30e-6, 40e-6 },
                new string[] { "d", "g", "s", "b" });
        }

        [TestMethod]
        public void BSIM2Test()
        {
            var netlist = Run(".model lvl5mod nmos level = 5 vfb = 0 lvfb = 1 wvfb = 2 phi = 3 lphi = 4 wphi = 5",
                "+ k1 = 6 lk1 = 7 wk1 = 8 k2 = 9 lk2 = 10 wk2 = 11 eta0 = 12 leta0 = 13 weta0 = 14",
                "+ etab = 15 letab = 16 wetab = 17 dl = 18 dw = 19 mu0 = 20 mu0b = 21 lmu0b = 22 wmu0b = 23",
                "+ mus0 = 24 lmus0 = 25 wmus0 = 26 musb = 27 lmusb = 28 wmusb = 29 mu20 = 30 lmu20 = 31",
                "+ wmu20 = 32 mu2b = 33 lmu2b = 34 wmu2b = 35 mu2g = 36 lmu2g = 37 wmu2g = 38 mu30 = 39",
                "+ lmu30 = 40 wmu30 = 41 mu3b = 42 lmu3b = 43 wmu3b = 44 mu3g = 45 lmu3g = 46 wmu3g = 47",
                "+ mu40 = 48 lmu40 = 49 wmu40 = 50 mu4b = 51 lmu4b = 52 wmu4b = 53 mu4g = 54 lmu4g = 55",
                "+ wmu4g = 56 ua0 = 57 lua0 = 58 wua0 = 59 uab = 60 luab = 61 wuab = 62 ub0 = 63 lub0 = 64",
                "+ wub0 = 65 ubb = 66 lubb = 67 wubb = 68 u10 = 69 lu10 = 70 wu10 = 71 u1b = 72 lu1b = 73",
                "+ wu1b = 74 u1d = 75 lu1d = 76 wu1d = 77 n0 = 78 ln0 = 79 wn0 = 80 nb = 81 lnb = 82 wnb = 83",
                "+ nd = 84 lnd = 85 wnd = 86 vof0 = 87 lvof0 = 88 wvof0 = 89 vofb = 90 lvofb = 91 wvofb = 92",
                "+ vofd = 93 lvofd = 94 wvofd = 95 ai0 = 96 lai0 = 97 wai0 = 98 aib = 99 laib = 100 waib = 101",
                "+ bi0 = 102 lbi0 = 103 wbi0 = 104 bib = 105 lbib = 106 wbib = 107 vghigh = 108 lvghigh = 109",
                "+ wvghigh = 110 vglow = 111 lvglow = 112 wvglow = 113 tox = 114 temp = 115 vdd = 116 vgg = 117",
                "+ vbb = 118 cgso = 119 cgdo = 120 cgbo = 121 xpart = 122 rsh = 123 js = 124 pb = 125 mj = 126",
                "+ pbsw = 127 mjsw = 128 cj = 129 cjsw = 130 wdf = 131 dell = 132",
                "MB d g s b lvl5mod w = 10u l = 11u ad = 1e-12 as = 2e-12 nrs = 2 nrd = 3 pd = 30u ps = 40u");
            Test<BSIM2Model>(netlist, "lvl5mod", new string[]
            {
                "vfb", "lvfb", "wvfb", "phi", "lphi", "wphi", "k1", "lk1", "wk1", "k2", "lk2", "wk2", "eta0",
                "leta0", "weta0", "etab", "letab", "wetab", "dl", "dw", "mu0", "mu0b", "lmu0b", "wmu0b", "mus0",
                "lmus0", "wmus0", "musb", "lmusb", "wmusb", "mu20", "lmu20", "wmu20", "mu2b", "lmu2b", "wmu2b",
                "mu2g", "lmu2g", "wmu2g", "mu30", "lmu30", "wmu30", "mu3b", "lmu3b", "wmu3b", "mu3g", "lmu3g",
                "wmu3g", "mu40", "lmu40", "wmu40", "mu4b", "lmu4b", "wmu4b", "mu4g", "lmu4g", "wmu4g", "ua0",
                "lua0", "wua0", "uab", "luab", "wuab", "ub0", "lub0", "wub0", "ubb", "lubb", "wubb", "u10",
                "lu10", "wu10", "u1b", "lu1b", "wu1b", "u1d", "lu1d", "wu1d", "n0", "ln0", "wn0", "nb", "lnb",
                "wnb", "nd", "lnd", "wnd", "vof0", "lvof0", "wvof0", "vofb", "lvofb", "wvofb", "vofd", "lvofd",
                "wvofd", "ai0", "lai0", "wai0", "aib", "laib", "waib", "bi0", "lbi0", "wbi0", "bib", "lbib", "wbib",
                "vghigh", "lvghigh", "wvghigh", "vglow", "lvglow", "wvglow", "tox", "temp", "vdd", "vgg", "vbb",
                "cgso", "cgdo", "cgbo", "xpart", "rsh", "js", "pb", "mj", "pbsw", "mjsw", "cj", "cjsw", "wdf", "dell"
            }, new double[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
                26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50,
                51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75,
                76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100,
                101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125,
                126, 127, 128, 129, 130, 131, 132
            });
            Test<BSIM2>(netlist, "MB",
                new string[] { "w", "l", "ad", "as", "nrs", "nrd", "pd", "ps" },
                new double[] { 10e-6, 11e-6, 1e-12, 2e-12, 2.0, 3.0, 30e-6, 40e-6 },
                new string[] { "d", "g", "s", "b" });
        }

        [TestMethod]
        public void BSIM3v24Test()
        {
            var netlist = Run(".model lvl49mod pmos(level = 7 mobmod = 0 binunit = 1 paramchk = 2 capmod = 3",
                "+ noimod = 4 version = 3.24 tox = 6 toxm = 7 cdsc = 8 cdscb = 9 cdscd = 10 cit = 11 nfactor = 12",
                "+ xj = 13 vsat = 14 a0 = 15 ags = 16 a1 = 17 a2 = 18 at = 19 keta = 20 nsub = 21 nch = 22",
                "+ ngate = 23 gamma1 = 24 gamma2 = 25 vbx = 26 vbm = 27 xt = 28 k1 = 29 kt1 = 30 kt1l = 31",
                "+ kt2 = 32 k2 = 33 k3 = 34 k3b = 35 nlx = 36 w0 = 37 dvt0 = 38 dvt1 = 39 dvt2 = 40 dvt0w = 41",
                "+ dvt1w = 42 dvt2w = 43 drout = 44 dsub = 45 vth0 = 46 ua = 47 ua1 = 48 ub = 49 ub1 = 50",
                "+ uc = 51 uc1 = 52 u0 = 53 ute = 54 voff = 55 delta = 56 rdsw = 57 prwg = 58 prwb = 59 prt = 60",
                "+ eta0 = 61 etab = 62 pclm = 63 pdiblc1 = 64 pdiblc2 = 65 pdiblcb = 66 pscbe1 = 67 pscbe2 = 68",
                "+ pvag = 69 wr = 70 dwg = 71 dwb = 72 b0 = 73 b1 = 74 alpha0 = 75 alpha1 = 76 beta0 = 77 ijth = 78",
                "+ vfb = 79 elm = 80 cgsl = 81 cgdl = 82 ckappa = 83 cf = 84 clc = 85 cle = 86 dwc = 87 dlc = 88",
                "+ vfbcv = 89 acde = 90 moin = 91 noff = 92 voffcv = 93 tcj = 94 tpb = 95 tcjsw = 96 tpbsw = 97",
                "+ tcjswg = 98 tpbswg = 99 lcdsc = 100 lcdscb = 101 lcdscd = 102 lcit = 103 lnfactor = 104",
                "+ lxj = 105 lvsat = 106 la0 = 107 lags = 108 la1 = 109 la2 = 110 lat = 111 lketa = 112 lnsub = 113",
                "+ lnch = 114 lngate = 115 lgamma1 = 116 lgamma2 = 117 lvbx = 118 lvbm = 119 lxt = 120 lk1 = 121",
                "+ lkt1 = 122 lkt1l = 123 lkt2 = 124 lk2 = 125 lk3 = 126 lk3b = 127 lnlx = 128 lw0 = 129 ldvt0 = 130",
                "+ ldvt1 = 131 ldvt2 = 132 ldvt0w = 133 ldvt1w = 134 ldvt2w = 135 ldrout = 136 ldsub = 137",
                "+ lvth0 = 138 lua = 139 lua1 = 140 lub = 141 lub1 = 142 luc = 143 luc1 = 144 lu0 = 145 lute = 146",
                "+ lvoff = 147 ldelta = 148 lrdsw = 149 lprwb = 150 lprwg = 151 lprt = 152 leta0 = 153 letab = 154",
                "+ lpclm = 155 lpdiblc1 = 156 lpdiblc2 = 157 lpdiblcb = 158 lpscbe1 = 159 lpscbe2 = 160 lpvag = 161",
                "+ lwr = 162 ldwg = 163 ldwb = 164 lb0 = 165 lb1 = 166 lalpha0 = 167 lalpha1 = 168 lbeta0 = 169",
                "+ lvfb = 170 lelm = 171 lcgsl = 172 lcgdl = 173 lckappa = 174 lcf = 175 lclc = 176 lcle = 177",
                "+ lvfbcv = 178 lacde = 179 lmoin = 180 lnoff = 181 lvoffcv = 182 wcdsc = 183 wcdscb = 184 wcdscd = 185",
                "+ wcit = 186 wnfactor = 187 wxj = 188 wvsat = 189 wa0 = 190 wags = 191 wa1 = 192 wa2 = 193 wat = 194",
                "+ wketa = 195 wnsub = 196 wnch = 197 wngate = 198 wgamma1 = 199 wgamma2 = 200 wvbx = 201 wvbm = 202",
                "+ wxt = 203 wk1 = 204 wkt1 = 205 wkt1l = 206 wkt2 = 207 wk2 = 208 wk3 = 209 wk3b = 210 wnlx = 211",
                "+ ww0 = 212 wdvt0 = 213 wdvt1 = 214 wdvt2 = 215 wdvt0w = 216 wdvt1w = 217 wdvt2w = 218 wdrout = 219",
                "+ wdsub = 220 wvth0 = 221 wua = 222 wua1 = 223 wub = 224 wub1 = 225 wuc = 226 wuc1 = 227 wu0 = 228",
                "+ wute = 229 wvoff = 230 wdelta = 231 wrdsw = 232 wprwb = 233 wprwg = 234 wprt = 235 weta0 = 236",
                "+ wetab = 237 wpclm = 238 wpdiblc1 = 239 wpdiblc2 = 240 wpdiblcb = 241 wpscbe1 = 242 wpscbe2 = 243",
                "+ wpvag = 244 wwr = 245 wdwg = 246 wdwb = 247 wb0 = 248 wb1 = 249 walpha0 = 250 walpha1 = 251",
                "+ wbeta0 = 252 wvfb = 253 welm = 254 wcgsl = 255 wcgdl = 256 wckappa = 257 wcf = 258 wclc = 259",
                "+ wcle = 260 wvfbcv = 261 wacde = 262 wmoin = 263 wnoff = 264 wvoffcv = 265 pcdsc = 266 pcdscb = 267",
                "+ pcdscd = 268 pcit = 269 pnfactor = 270 pxj = 271 pvsat = 272 pa0 = 273 pags = 274 pa1 = 275",
                "+ pa2 = 276 pat = 277 pketa = 278 pnsub = 279 pnch = 280 pngate = 281 pgamma1 = 282 pgamma2 = 283",
                "+ pvbx = 284 pvbm = 285 pxt = 286 pk1 = 287 pkt1 = 288 pkt1l = 289 pkt2 = 290 pk2 = 291 pk3 = 292",
                "+ pk3b = 293 pnlx = 294 pw0 = 295 pdvt0 = 296 pdvt1 = 297 pdvt2 = 298 pdvt0w = 299 pdvt1w = 300",
                "+ pdvt2w = 301 pdrout = 302 pdsub = 303 pvth0 = 304 pua = 305 pua1 = 306 pub = 307 pub1 = 308",
                "+ puc = 309 puc1 = 310 pu0 = 311 pute = 312 pvoff = 313 pdelta = 314 prdsw = 315 pprwb = 316",
                "+ pprwg = 317 pprt = 318 peta0 = 319 petab = 320 ppclm = 321 ppdiblc1 = 322 ppdiblc2 = 323",
                "+ ppdiblcb = 324 ppscbe1 = 325 ppscbe2 = 326 ppvag = 327 pwr = 328 pdwg = 329 pdwb = 330 pb0 = 331",
                "+ pb1 = 332 palpha0 = 333 palpha1 = 334 pbeta0 = 335 pvfb = 336 pelm = 337 pcgsl = 338 pcgdl = 339",
                "+ pckappa = 340 pcf = 341 pclc = 342 pcle = 343 pvfbcv = 344 pacde = 345 pmoin = 346 pnoff = 347",
                "+ pvoffcv = 348 tnom = 349 cgso = 350 cgdo = 351 cgbo = 352 xpart = 353 rsh = 354 js = 355 jsw = 356",
                "+ pb = 357 mj = 358 pbsw = 359 mjsw = 360 cj = 361 cjsw = 362 nj = 363 pbswg = 364 mjswg = 365",
                "+ cjswg = 366 xti = 367 lint = 368 ll = 369 llc = 370 lln = 371 lw = 372 lwc = 373 lwn = 374 lwl = 375",
                "+ lwlc = 376 lmin = 377 lmax = 378 wint = 379 wl = 380 wlc = 381 wln = 382 ww = 383 wwc = 384 wwn = 385",
                "+ wwl = 386 wwlc = 387 wmin = 388 wmax = 389 noia = 390 noib = 391 noic = 392 em = 393 ef = 394",
                "+ af = 395 kf = 396)",
                "M1 dr g src blk lvl49mod w = 10u l = 11u ad = 1e-12 as = 2e-12 nrs = 2 nrd = 3 pd = 30u ps = 40u");
            Test<BSIM3v24Model>(netlist, "lvl49mod", new string[]
            {
                "mobmod", "binunit", "paramchk", "capmod", "noimod", "tox", "toxm", "cdsc", "cdscb", "cdscd", "cit",
                "nfactor", "xj", "vsat", "a0", "ags", "a1", "a2", "at", "keta", "nsub", "nch", "ngate", "gamma1",
                "gamma2", "vbx", "vbm", "xt", "k1", "kt1", "kt1l", "kt2", "k2", "k3", "k3b", "nlx", "w0", "dvt0",
                "dvt1", "dvt2", "dvt0w", "dvt1w", "dvt2w", "drout", "dsub", "vth0", "ua", "ua1", "ub", "ub1", "uc",
                "uc1", "u0", "ute", "voff", "delta", "rdsw", "prwg", "prwb", "prt", "eta0", "etab", "pclm", "pdiblc1",
                "pdiblc2", "pdiblcb", "pscbe1", "pscbe2", "pvag", "wr", "dwg", "dwb", "b0", "b1", "alpha0", "alpha1",
                "beta0", "ijth", "vfb", "elm", "cgsl", "cgdl", "ckappa", "cf", "clc", "cle", "dwc", "dlc", "vfbcv",
                "acde", "moin", "noff", "voffcv", "tcj", "tpb", "tcjsw", "tpbsw", "tcjswg", "tpbswg", "lcdsc", "lcdscb",
                "lcdscd", "lcit", "lnfactor", "lxj", "lvsat", "la0", "lags", "la1", "la2", "lat", "lketa", "lnsub",
                "lnch", "lngate", "lgamma1", "lgamma2", "lvbx", "lvbm", "lxt", "lk1", "lkt1", "lkt1l", "lkt2", "lk2",
                "lk3", "lk3b", "lnlx", "lw0", "ldvt0", "ldvt1", "ldvt2", "ldvt0w", "ldvt1w", "ldvt2w", "ldrout",
                "ldsub", "lvth0", "lua", "lua1", "lub", "lub1", "luc", "luc1", "lu0", "lute", "lvoff", "ldelta",
                "lrdsw", "lprwb", "lprwg", "lprt", "leta0", "letab", "lpclm", "lpdiblc1", "lpdiblc2", "lpdiblcb",
                "lpscbe1", "lpscbe2", "lpvag", "lwr", "ldwg", "ldwb", "lb0", "lb1", "lalpha0", "lalpha1", "lbeta0",
                "lvfb", "lelm", "lcgsl", "lcgdl", "lckappa", "lcf", "lclc", "lcle", "lvfbcv", "lacde", "lmoin",
                "lnoff", "lvoffcv", "wcdsc", "wcdscb", "wcdscd", "wcit", "wnfactor", "wxj", "wvsat", "wa0", "wags",
                "wa1", "wa2", "wat", "wketa", "wnsub", "wnch", "wngate", "wgamma1", "wgamma2", "wvbx", "wvbm", "wxt",
                "wk1", "wkt1", "wkt1l", "wkt2", "wk2", "wk3", "wk3b", "wnlx", "ww0", "wdvt0", "wdvt1", "wdvt2", "wdvt0w",
                "wdvt1w", "wdvt2w", "wdrout", "wdsub", "wvth0", "wua", "wua1", "wub", "wub1", "wuc", "wuc1", "wu0",
                "wute", "wvoff", "wdelta", "wrdsw", "wprwb", "wprwg", "wprt", "weta0", "wetab", "wpclm", "wpdiblc1",
                "wpdiblc2", "wpdiblcb", "wpscbe1", "wpscbe2", "wpvag", "wwr", "wdwg", "wdwb", "wb0", "wb1", "walpha0",
                "walpha1", "wbeta0", "wvfb", "welm", "wcgsl", "wcgdl", "wckappa", "wcf", "wclc", "wcle", "wvfbcv",
                "wacde", "wmoin", "wnoff", "wvoffcv", "pcdsc", "pcdscb", "pcdscd", "pcit", "pnfactor", "pxj", "pvsat",
                "pa0", "pags", "pa1", "pa2", "pat", "pketa", "pnsub", "pnch", "pngate", "pgamma1", "pgamma2", "pvbx",
                "pvbm", "pxt", "pk1", "pkt1", "pkt1l", "pkt2", "pk2", "pk3", "pk3b", "pnlx", "pw0", "pdvt0", "pdvt1",
                "pdvt2", "pdvt0w", "pdvt1w", "pdvt2w", "pdrout", "pdsub", "pvth0", "pua", "pua1", "pub", "pub1", "puc",
                "puc1", "pu0", "pute", "pvoff", "pdelta", "prdsw", "pprwb", "pprwg", "pprt", "peta0", "petab", "ppclm",
                "ppdiblc1", "ppdiblc2", "ppdiblcb", "ppscbe1", "ppscbe2", "ppvag", "pwr", "pdwg", "pdwb", "pb0", "pb1",
                "palpha0", "palpha1", "pbeta0", "pvfb", "pelm", "pcgsl", "pcgdl", "pckappa", "pcf", "pclc", "pcle", "pvfbcv",
                "pacde", "pmoin", "pnoff", "pvoffcv", "tnom", "cgso", "cgdo", "cgbo", "xpart", "rsh", "js", "jsw", "pb", "mj",
                "pbsw", "mjsw", "cj", "cjsw", "nj", "pbswg", "mjswg", "cjswg", "xti", "lint", "ll", "llc", "lln", "lw",
                "lwc", "lwn", "lwl", "lwlc", "lmin", "lmax", "wint", "wl", "wlc", "wln", "ww", "wwc", "wwn", "wwl", "wwlc",
                "wmin", "wmax", "noia", "noib", "noic", "em", "ef", "af", "kf"
            }, new double[]
            {
                    0, 1, 2, 3, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
                26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
                50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73,
                74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97,
                98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117,
                118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136,
                137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155,
                156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174,
                175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193,
                194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212,
                213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 229, 230, 231,
                232, 233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250,
                251, 252, 253, 254, 255, 256, 257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 267, 268, 269,
                270, 271, 272, 273, 274, 275, 276, 277, 278, 279, 280, 281, 282, 283, 284, 285, 286, 287, 288,
                289, 290, 291, 292, 293, 294, 295, 296, 297, 298, 299, 300, 301, 302, 303, 304, 305, 306, 307,
                308, 309, 310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 320, 321, 322, 323, 324, 325, 326,
                327, 328, 329, 330, 331, 332, 333, 334, 335, 336, 337, 338, 339, 340, 341, 342, 343, 344, 345,
                346, 347, 348, 349, 350, 351, 352, 353, 354, 355, 356, 357, 358, 359, 360, 361, 362, 363, 364,
                365, 366, 367, 368, 369, 370, 371, 372, 373, 374, 375, 376, 377, 378, 379, 380, 381, 382, 383,
                384, 385, 386, 387, 388, 389, 390, 391, 392, 393, 394, 395, 396
            });
            Test<BSIM3v24>(netlist, "M1",
                new string[] { "w", "l", "ad", "as", "nrs", "nrd", "pd", "ps" },
                new double[] { 10e-6, 11e-6, 1e-12, 2e-12, 2.0, 3.0, 30e-6, 40e-6 },
                new string[] { "dr", "g", "src", "blk" });
        }

        [TestMethod]
        public void BSIM3v30Test()
        {
            var netlist = Run(".model lvl49mod nmos level = 7 version = 3.3.0 mobmod = 0 binunit = 1 paramchk = 2 capmod = 3",
                "+ noimod = 4 acnqsmod = 5 tox = 7 toxm = 8 cdsc = 9 cdscb = 10 cdscd = 11 cit = 12",
                "+ nfactor = 13 xj = 14 vsat = 15 a0 = 16 ags = 17 a1 = 18 a2 = 19 at = 20 keta = 21 nsub = 22",
                "+ nch = 23 ngate = 24 gamma1 = 25 gamma2 = 26 vbx = 27 vbm = 28 xt = 29 k1 = 30 kt1 = 31 kt1l = 32",
                "+ kt2 = 33 k2 = 34 k3 = 35 k3b = 36 nlx = 37 w0 = 38 dvt0 = 39 dvt1 = 40 dvt2 = 41 dvt0w = 42",
                "+ dvt1w = 43 dvt2w = 44 drout = 45 dsub = 46 vth0 = 47 ua = 48 ua1 = 49 ub = 50 ub1 = 51 uc = 52",
                "+ uc1 = 53 u0 = 54 ute = 55 voff = 56 delta = 57 rdsw = 58 prwg = 59 prwb = 60 prt = 61 eta0 = 62",
                "+ etab = 63 pclm = 64 pdiblc1 = 65 pdiblc2 = 66 pdiblcb = 67 pscbe1 = 68 pscbe2 = 69 pvag = 70",
                "+ wr = 71 dwg = 72 dwb = 73 b0 = 74 b1 = 75 alpha0 = 76 alpha1 = 77 beta0 = 78 ijth = 79 vfb = 80",
                "+ elm = 81 cgsl = 82 cgdl = 83 ckappa = 84 cf = 85 clc = 86 cle = 87 dwc = 88 dlc = 89 vfbcv = 90",
                "+ acde = 91 moin = 92 noff = 93 voffcv = 94 tcj = 95 tpb = 96 tcjsw = 97 tpbsw = 98 tcjswg = 99",
                "+ tpbswg = 100 lcdsc = 101 lcdscb = 102 lcdscd = 103 lcit = 104 lnfactor = 105 lxj = 106 lvsat = 107",
                "+ la0 = 108 lags = 109 la1 = 110 la2 = 111 lat = 112 lketa = 113 lnsub = 114 lnch = 115 lngate = 116",
                "+ lgamma1 = 117 lgamma2 = 118 lvbx = 119 lvbm = 120 lxt = 121 lk1 = 122 lkt1 = 123 lkt1l = 124",
                "+ lkt2 = 125 lk2 = 126 lk3 = 127 lk3b = 128 lnlx = 129 lw0 = 130 ldvt0 = 131 ldvt1 = 132 ldvt2 = 133",
                "+ ldvt0w = 134 ldvt1w = 135 ldvt2w = 136 ldrout = 137 ldsub = 138 lvth0 = 139 lua = 140 lua1 = 141",
                "+ lub = 142 lub1 = 143 luc = 144 luc1 = 145 lu0 = 146 lute = 147 lvoff = 148 ldelta = 149 lrdsw = 150",
                "+ lprwb = 151 lprwg = 152 lprt = 153 leta0 = 154 letab = 155 lpclm = 156 lpdiblc1 = 157 lpdiblc2 = 158",
                "+ lpdiblcb = 159 lpscbe1 = 160 lpscbe2 = 161 lpvag = 162 lwr = 163 ldwg = 164 ldwb = 165 lb0 = 166",
                "+ lb1 = 167 lalpha0 = 168 lalpha1 = 169 lbeta0 = 170 lvfb = 171 lelm = 172 lcgsl = 173 lcgdl = 174",
                "+ lckappa = 175 lcf = 176 lclc = 177 lcle = 178 lvfbcv = 179 lacde = 180 lmoin = 181 lnoff = 182",
                "+ lvoffcv = 183 wcdsc = 184 wcdscb = 185 wcdscd = 186 wcit = 187 wnfactor = 188 wxj = 189 wvsat = 190",
                "+ wa0 = 191 wags = 192 wa1 = 193 wa2 = 194 wat = 195 wketa = 196 wnsub = 197 wnch = 198 wngate = 199",
                "+ wgamma1 = 200 wgamma2 = 201 wvbx = 202 wvbm = 203 wxt = 204 wk1 = 205 wkt1 = 206 wkt1l = 207",
                "+ wkt2 = 208 wk2 = 209 wk3 = 210 wk3b = 211 wnlx = 212 ww0 = 213 wdvt0 = 214 wdvt1 = 215 wdvt2 = 216",
                "+ wdvt0w = 217 wdvt1w = 218 wdvt2w = 219 wdrout = 220 wdsub = 221 wvth0 = 222 wua = 223 wua1 = 224",
                "+ wub = 225 wub1 = 226 wuc = 227 wuc1 = 228 wu0 = 229 wute = 230 wvoff = 231 wdelta = 232 wrdsw = 233",
                "+ wprwb = 234 wprwg = 235 wprt = 236 weta0 = 237 wetab = 238 wpclm = 239 wpdiblc1 = 240 wpdiblc2 = 241",
                "+ wpdiblcb = 242 wpscbe1 = 243 wpscbe2 = 244 wpvag = 245 wwr = 246 wdwg = 247 wdwb = 248 wb0 = 249",
                "+ wb1 = 250 walpha0 = 251 walpha1 = 252 wbeta0 = 253 wvfb = 254 welm = 255 wcgsl = 256 wcgdl = 257",
                "+ wckappa = 258 wcf = 259 wclc = 260 wcle = 261 wvfbcv = 262 wacde = 263 wmoin = 264 wnoff = 265",
                "+ wvoffcv = 266 pcdsc = 267 pcdscb = 268 pcdscd = 269 pcit = 270 pnfactor = 271 pxj = 272 pvsat = 273",
                "+ pa0 = 274 pags = 275 pa1 = 276 pa2 = 277 pat = 278 pketa = 279 pnsub = 280 pnch = 281 pngate = 282",
                "+ pgamma1 = 283 pgamma2 = 284 pvbx = 285 pvbm = 286 pxt = 287 pk1 = 288 pkt1 = 289 pkt1l = 290",
                "+ pkt2 = 291 pk2 = 292 pk3 = 293 pk3b = 294 pnlx = 295 pw0 = 296 pdvt0 = 297 pdvt1 = 298 pdvt2 = 299",
                "+ pdvt0w = 300 pdvt1w = 301 pdvt2w = 302 pdrout = 303 pdsub = 304 pvth0 = 305 pua = 306 pua1 = 307",
                "+ pub = 308 pub1 = 309 puc = 310 puc1 = 311 pu0 = 312 pute = 313 pvoff = 314 pdelta = 315 prdsw = 316",
                "+ pprwb = 317 pprwg = 318 pprt = 319 peta0 = 320 petab = 321 ppclm = 322 ppdiblc1 = 323 ppdiblc2 = 324",
                "+ ppdiblcb = 325 ppscbe1 = 326 ppscbe2 = 327 ppvag = 328 pwr = 329 pdwg = 330 pdwb = 331 pb0 = 332",
                "+ pb1 = 333 palpha0 = 334 palpha1 = 335 pbeta0 = 336 pvfb = 337 pelm = 338 pcgsl = 339 pcgdl = 340",
                "+ pckappa = 341 pcf = 342 pclc = 343 pcle = 344 pvfbcv = 345 pacde = 346 pmoin = 347 pnoff = 348",
                "+ pvoffcv = 349 tnom = 350 cgso = 351 cgdo = 352 cgbo = 353 xpart = 354 rsh = 355 js = 356 jsw = 357",
                "+ pb = 358 mj = 359 pbsw = 360 mjsw = 361 cj = 362 cjsw = 363 nj = 364 pbswg = 365 mjswg = 366",
                "+ cjswg = 367 xti = 368 lintnoi = 369 lint = 370 ll = 371 llc = 372 lln = 373 lw = 374 lwc = 375",
                "+ lwn = 376 lwl = 377 lwlc = 378 lmin = 379 lmax = 380 wint = 381 wl = 382 wlc = 383 wln = 384 ww = 385",
                "+ wwc = 386 wwn = 387 wwl = 388 wwlc = 389 wmin = 390 wmax = 391 noia = 392 noib = 393 noic = 394 ",
                "+ em = 395 ef = 396 af = 397 kf = 398",
                "M1 dr g src blk lvl49mod w = 10u l = 11u ad = 1e-12 as = 2e-12 nrs = 2 nrd = 3 pd = 30u ps = 40u");
            Test<BSIM3v30Model>(netlist, "lvl49mod", new string[]
            {
                "mobmod", "binunit", "paramchk", "capmod",
                "noimod", "acnqsmod", "tox", "toxm", "cdsc", "cdscb", "cdscd", "cit",
                "nfactor", "xj", "vsat", "a0", "ags", "a1", "a2", "at", "keta", "nsub", "nch",
                "ngate", "gamma1", "gamma2", "vbx", "vbm", "xt", "k1", "kt1", "kt1l", "kt2", "k2",
                "k3", "k3b", "nlx", "w0", "dvt0", "dvt1", "dvt2", "dvt0w", "dvt1w", "dvt2w", "drout",
                "dsub", "vth0", "ua", "ua1", "ub", "ub1", "uc", "uc1", "u0", "ute", "voff", "delta",
                "rdsw", "prwg", "prwb", "prt", "eta0", "etab", "pclm", "pdiblc1", "pdiblc2",
                "pdiblcb", "pscbe1", "pscbe2", "pvag", "wr", "dwg", "dwb", "b0", "b1", "alpha0",
                "alpha1", "beta0", "ijth", "vfb", "elm", "cgsl", "cgdl", "ckappa", "cf", "clc",
                "cle", "dwc", "dlc", "vfbcv", "acde", "moin", "noff", "voffcv", "tcj", "tpb", "tcjsw",
                "tpbsw", "tcjswg", "tpbswg", "lcdsc", "lcdscb", "lcdscd", "lcit", "lnfactor", "lxj",
                "lvsat", "la0", "lags", "la1", "la2", "lat", "lketa", "lnsub", "lnch", "lngate",
                "lgamma1", "lgamma2", "lvbx", "lvbm", "lxt", "lk1", "lkt1", "lkt1l", "lkt2", "lk2",
                "lk3", "lk3b", "lnlx", "lw0", "ldvt0", "ldvt1", "ldvt2", "ldvt0w", "ldvt1w", "ldvt2w",
                "ldrout", "ldsub", "lvth0", "lua", "lua1", "lub", "lub1", "luc", "luc1", "lu0", "lute",
                "lvoff", "ldelta", "lrdsw", "lprwb", "lprwg", "lprt", "leta0", "letab", "lpclm",
                "lpdiblc1", "lpdiblc2", "lpdiblcb", "lpscbe1", "lpscbe2", "lpvag", "lwr", "ldwg",
                "ldwb", "lb0", "lb1", "lalpha0", "lalpha1", "lbeta0", "lvfb", "lelm", "lcgsl", "lcgdl",
                "lckappa", "lcf", "lclc", "lcle", "lvfbcv", "lacde", "lmoin", "lnoff", "lvoffcv",
                "wcdsc", "wcdscb", "wcdscd", "wcit", "wnfactor", "wxj", "wvsat", "wa0", "wags", "wa1",
                "wa2", "wat", "wketa", "wnsub", "wnch", "wngate", "wgamma1", "wgamma2", "wvbx", "wvbm",
                "wxt", "wk1", "wkt1", "wkt1l", "wkt2", "wk2", "wk3", "wk3b", "wnlx", "ww0", "wdvt0",
                "wdvt1", "wdvt2", "wdvt0w", "wdvt1w", "wdvt2w", "wdrout", "wdsub", "wvth0", "wua", "wua1",
                "wub", "wub1", "wuc", "wuc1", "wu0", "wute", "wvoff", "wdelta", "wrdsw", "wprwb", "wprwg",
                "wprt", "weta0", "wetab", "wpclm", "wpdiblc1", "wpdiblc2", "wpdiblcb", "wpscbe1",
                "wpscbe2", "wpvag", "wwr", "wdwg", "wdwb", "wb0", "wb1", "walpha0", "walpha1", "wbeta0",
                "wvfb", "welm", "wcgsl", "wcgdl", "wckappa", "wcf", "wclc", "wcle", "wvfbcv", "wacde",
                "wmoin", "wnoff", "wvoffcv", "pcdsc", "pcdscb", "pcdscd", "pcit", "pnfactor", "pxj",
                "pvsat", "pa0", "pags", "pa1", "pa2", "pat", "pketa", "pnsub", "pnch", "pngate", "pgamma1",
                "pgamma2", "pvbx", "pvbm", "pxt", "pk1", "pkt1", "pkt1l", "pkt2", "pk2", "pk3", "pk3b",
                "pnlx", "pw0", "pdvt0", "pdvt1", "pdvt2", "pdvt0w", "pdvt1w", "pdvt2w", "pdrout", "pdsub",
                "pvth0", "pua", "pua1", "pub", "pub1", "puc", "puc1", "pu0", "pute", "pvoff", "pdelta",
                "prdsw", "pprwb", "pprwg", "pprt", "peta0", "petab", "ppclm", "ppdiblc1", "ppdiblc2",
                "ppdiblcb", "ppscbe1", "ppscbe2", "ppvag", "pwr", "pdwg", "pdwb", "pb0", "pb1", "palpha0",
                "palpha1", "pbeta0", "pvfb", "pelm", "pcgsl", "pcgdl", "pckappa", "pcf", "pclc", "pcle",
                "pvfbcv", "pacde", "pmoin", "pnoff", "pvoffcv", "tnom", "cgso", "cgdo", "cgbo", "xpart",
                "rsh", "js", "jsw", "pb", "mj", "pbsw", "mjsw", "cj", "cjsw", "nj", "pbswg", "mjswg", "cjswg",
                "xti", "lintnoi", "lint", "ll", "llc", "lln", "lw", "lwc", "lwn", "lwl", "lwlc", "lmin",
                "lmax", "wint", "wl", "wlc", "wln", "ww", "wwc", "wwn", "wwl", "wwlc", "wmin", "wmax", "noia",
                "noib", "noic", "em", "ef", "af", "kf"
            }, new double[] 
            {
                0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21,
                22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41,
                42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61,
                62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81,
                82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101,
                102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117,
                118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133,
                134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149,
                150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165,
                166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181,
                182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197,
                198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213,
                214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 229,
                230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245,
                246, 247, 248, 249, 250, 251, 252, 253, 254, 255, 256, 257, 258, 259, 260, 261,
                262, 263, 264, 265, 266, 267, 268, 269, 270, 271, 272, 273, 274, 275, 276, 277,
                278, 279, 280, 281, 282, 283, 284, 285, 286, 287, 288, 289, 290, 291, 292, 293,
                294, 295, 296, 297, 298, 299, 300, 301, 302, 303, 304, 305, 306, 307, 308, 309,
                310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 320, 321, 322, 323, 324, 325,
                326, 327, 328, 329, 330, 331, 332, 333, 334, 335, 336, 337, 338, 339, 340, 341,
                342, 343, 344, 345, 346, 347, 348, 349, 350, 351, 352, 353, 354, 355, 356, 357,
                358, 359, 360, 361, 362, 363, 364, 365, 366, 367, 368, 369, 370, 371, 372, 373,
                374, 375, 376, 377, 378, 379, 380, 381, 382, 383, 384, 385, 386, 387, 388, 389,
                390, 391, 392, 393, 394, 395, 396, 397, 398
            });
            Test<BSIM3v30>(netlist, "M1", 
                new string[] { "w", "l", "ad", "as", "nrs", "nrd", "pd", "ps" }, 
                new double[] { 10e-6, 11e-6, 1e-12, 2e-12, 2.0, 3.0, 30e-6, 40e-6 }, 
                new string[] { "dr", "g", "src", "blk" });
        }
    }
}
