using System;

namespace SpiceSharp.Sparse
{
    public static class spdefs
    {
        public static double ELEMENT_MAG(MatrixElement ptr) => Math.Abs(ptr.Real) + Math.Abs(ptr.Imag);

        /* Complex assignment statements. */
        public static void CMPLX_ASSIGN(MatrixElement to, MatrixElement from)
        {
            to.Real = from.Real;
            to.Imag = from.Imag;
        }
        public static void CMPLX_CONJ_ASSIGN(MatrixElement to, MatrixElement from)
        {
            to.Real = from.Real;
            to.Imag = -from.Imag;
        }
        public static void CMPLX_NEGATE_ASSIGN(MatrixElement to, MatrixElement from)
        {
            to.Real = -from.Real;
            to.Imag = -from.Imag;
        }
        public static void CMPLX_CONJ_NEGATE_ASSIGN(MatrixElement to, MatrixElement from)
        {
            to.Real = -from.Real;
            to.Imag = from.Imag;
        }
        public static void CMPLX_CONJ(MatrixElement a) => a.Imag = -a.Imag;
        public static void CMPLX_NEGATE(MatrixElement a)
        {
            a.Real = -a.Real;
            a.Imag = -a.Imag;
        }

        /* Macro that returns the approx magnitude (L-1 norm) of a complex number. */
        public static double CMPLX_1_NORM(MatrixElement a) => Math.Abs(a.Real) + Math.Abs(a.Imag);

        /* Macro that returns the approx magnitude (L-infinity norm) of a complex. */
        public static double CMPLX_INF_NORM(MatrixElement a) => Math.Max(Math.Abs(a.Real), Math.Abs(a.Imag));

        /* Macro function that returns the magnitude (L-2 norm) of a complex number. */
        public static void CMPLX_2_NORM(MatrixElement a) => Math.Sqrt(a.Real * a.Real + a.Imag * a.Imag);

        /* Macro function that performs complex addition. */
        public static void CMPLX_ADD(MatrixElement to, MatrixElement from_a, MatrixElement from_b)
        {
            to.Real = from_a.Real + from_b.Real;
            to.Imag = from_a.Imag + from_b.Imag;
        }

        /* Macro function that performs complex subtraction. */
        public static void CMPLX_SUBT(MatrixElement to, MatrixElement from_a, MatrixElement from_b)
        {
            to.Real = from_a.Real - from_b.Real;
            to.Imag = from_a.Imag - from_b.Imag;
        }

        /* Macro function that is equivalent to += operator for complex numbers. */
        public static void CMPLX_ADD_ASSIGN(MatrixElement to, MatrixElement from)
        {
            to.Real += from.Real;
            to.Imag += from.Imag;
        }

        /* Macro function that is equivalent to -= operator for complex numbers. */
        public static void CMPLX_SUBT_ASSIGN(MatrixElement to, MatrixElement from)
        {
            to.Real -= from.Real;
            to.Imag -= from.Imag;
        }

        /* Macro function that multiplies a complex number by a scalar. */
        public static void SCLR_MULT(MatrixElement to, double sclr, MatrixElement cmplx)
        {
            to.Real = sclr * cmplx.Real;
            to.Imag = sclr * cmplx.Imag;
        }

        /* Macro function that multiply-assigns a complex number by a scalar. */
        public static void SCLR_MULT_ASSIGN(MatrixElement to, MatrixElement sclr)
        {
            to.Real *= sclr;
            to.Imag *= sclr;
        }

        /* Macro function that multiplies two complex numbers. */
        public static void CMPLX_MULT(MatrixElement to, MatrixElement from_a, MatrixElement from_b)
        {
            to.Real = from_a.Real * from_b.Real -
                        from_a.Imag * from_b.Imag;
            to.Imag = from_a.Real * from_b.Imag +
                        from_a.Imag * from_b.Real;
        }

        /* Macro function that implements to *= from for complex numbers. */
        public static void CMPLX_MULT_ASSIGN(MatrixElement to, MatrixElement from)
        {
            double to_real_ = to.Real;
            to.Real = to_real_ * from.Real -
                        to.Imag * from.Imag;
            to.Imag = to_real_ * from.Imag +
                        to.Imag * from.Real;
        }

        /* Macro function that multiplies two complex numbers, the first of which is
         * conjugated. */
        public static void CMPLX_CONJ_MULT(MatrixElement to, MatrixElement from_a, MatrixElement from_b)
        {
            to.Real = from_a.Real * from_b.Real +
                from_a.Imag * from_b.Imag;
            to.Imag = from_a.Real * from_b.Imag -
                        from_a.Imag * from_b.Real;
        }

        /* Macro function that multiplies two complex numbers and then adds them
         * to another. to = add + mult_a * mult_b */
        public static void CMPLX_MULT_ADD(MatrixElement to, MatrixElement mult_a, MatrixElement mult_b, MatrixElement add)
        {
            to.Real = mult_a.Real * mult_b.Real -
                        mult_a.Imag * mult_b.Imag + add.Real;
            to.Imag = mult_a.Real * mult_b.Imag +
                        mult_a.Imag * mult_b.Real + add.Imag;
        }

        /* Macro function that subtracts the product of two complex numbers from
         * another.  to = subt - mult_a * mult_b */
        public static void CMPLX_MULT_SUBT(MatrixElement to, MatrixElement mult_a, MatrixElement mult_b, MatrixElement subt)
        {
            to.Real = subt.Real - mult_a.Real * mult_b.Real +
                                      mult_a.Imag * mult_b.Imag;
            to.Imag = subt.Imag - mult_a.Real * mult_b.Imag -
                                      mult_a.Imag * mult_b.Real;
        }

        /* Macro function that multiplies two complex numbers and then adds them
         * to another. to = add + mult_a* * mult_b where mult_a* represents mult_a
         * conjugate. */
        public static void CMPLX_CONJ_MULT_ADD(MatrixElement to, MatrixElement mult_a, MatrixElement mult_b, MatrixElement add)
        {
            to.Real = mult_a.Real * mult_b.Real +
                        mult_a.Imag * mult_b.Imag + add.Real;
            to.Imag = mult_a.Real * mult_b.Imag -
                        mult_a.Imag * mult_b.Real + add.Imag;
        }

        /* Macro function that multiplies two complex numbers and then adds them
         * to another. to += mult_a * mult_b */
        public static void CMPLX_MULT_ADD_ASSIGN(MatrixElement to, MatrixElement from_a, MatrixElement from_b)
        {
            to.Real += from_a.Real * from_b.Real -
                         from_a.Imag * from_b.Imag;
            to.Imag += from_a.Real * from_b.Imag +
                         from_a.Imag * from_b.Real;
        }

        /* Macro function that multiplies two complex numbers and then subtracts them
         * from another. */
        public static void CMPLX_MULT_SUBT_ASSIGN(MatrixElement to, MatrixElement from_a, MatrixElement from_b)
        {
            to.Real -= from_a.Real * from_b.Real -
                         from_a.Imag * from_b.Imag;
            to.Imag -= from_a.Real * from_b.Imag +
                         from_a.Imag * from_b.Real;
        }

        /* Macro function that multiplies two complex numbers and then adds them
         * to the destination. to += from_a* * from_b where from_a* represents from_a
         * conjugate. */
        public static void CMPLX_CONJ_MULT_ADD_ASSIGN(MatrixElement to, MatrixElement from_a, MatrixElement from_b)
        {
            to.Real += from_a.Real * from_b.Real +
                         from_a.Imag * from_b.Imag;
            to.Imag += from_a.Real * from_b.Imag -
                         from_a.Imag * from_b.Real;
        }

        /* Macro function that multiplies two complex numbers and then subtracts them
         * from the destination. to -= from_a* * from_b where from_a* represents from_a
         * conjugate. */
        public static void CMPLX_CONJ_MULT_SUBT_ASSIGN(MatrixElement to, MatrixElement from_a, MatrixElement from_b)
        {
            to.Real -= from_a.Real * from_b.Real +
                         from_a.Imag * from_b.Imag;
            to.Imag -= from_a.Real * from_b.Imag -
                         from_a.Imag * from_b.Real;
        }

        /*
         * Macro functions that provide complex division.
         */

        /* Complex division:  to = num / den */
        public static void CMPLX_DIV(MatrixElement to, MatrixElement num, MatrixElement den)
        {
            double r_, s_;
            if ((den.Real >= den.Imag && den.Real > -den.Imag) ||
                (den.Real < den.Imag && den.Real <= -den.Imag))
            {
                r_ = den.Imag / den.Real;
                s_ = den.Real + r_ * den.Imag;
                to.Real = (num.Real + r_ * num.Imag) / s_;
                to.Imag = (num.Imag - r_ * num.Real) / s_;
            }
            else
            {
                r_ = den.Real / den.Imag;
                s_ = den.Imag + r_ * den.Real;
                to.Real = (r_ * num.Real + num.Imag) / s_;
                to.Imag = (r_ * num.Imag - num.Real) / s_;
            }
        }

        /* Complex division and assignment:  num /= den */
        public static void CMPLX_DIV_ASSIGN(MatrixElement num, MatrixElement den)
        {
            double r_, s_, t_;
            if ((den.Real >= den.Imag && den.Real > -den.Imag) ||
                (den.Real < den.Imag && den.Real <= -den.Imag))
            {
                r_ = den.Imag / den.Real;
                s_ = den.Real + r_ * den.Imag;
                t_ = (num.Real + r_ * num.Imag) / s_;
                num.Imag = (num.Imag - r_ * num.Real) / s_;
                num.Real = t_;
            }
            else
            {
                r_ = den.Real / den.Imag;
                s_ = den.Imag + r_ * den.Real;
                t_ = (r_ * num.Real + num.Imag) / s_;
                num.Imag = (r_ * num.Imag - num.Real) / s_;
                num.Real = t_;
            }
        }

        /* Complex reciprocation:  to = 1.0 / den */
        public static void CMPLX_RECIPROCAL(MatrixElement to, MatrixElement den)
        {
            double r_;
            if ((den.Real >= den.Imag && den.Real > -den.Imag) ||
                (den.Real < den.Imag && den.Real <= -den.Imag))
            {
                r_ = den.Imag / den.Real;
                to.Imag = -r_ * (to.Real = 1.0 / (den.Real + r_ * den.Imag));
            }
            else
            {
                r_ = den.Real / den.Imag;
                to.Real = -r_ * (to.Imag = -1.0 / (den.Imag + r_ * den.Real));
            }
        }

        /* Macro function that returns the square of a number. */
        public static double SQR(double a) => a * a;

        /* Macro procedure that swaps two entities. */
        public static void SWAP(ref int a, ref int b)
        {
            int swapx = a;
            a = b;
            b = swapx;
        }
        public static void SWAP(ref long a, ref long b)
        {
            long swapx;
            swapx = a;
            a = b;
            b = swapx;
        }
        public static void SWAP(ref double a, ref double b)
        {
            double swapx;
            swapx = a;
            a = b;
            b = swapx;
        }
        public static void SWAP(ref MatrixElement a, ref MatrixElement b)
        {
            MatrixElement swapx;
            swapx = a;
            a = b;
            b = swapx;
        }
    }
}
