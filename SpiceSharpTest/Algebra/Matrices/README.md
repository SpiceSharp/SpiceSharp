# Test matrices

The `*.mtx.gz` files here are matrices from the **SuiteSparse Matrix Collection**
(<https://sparse.tamu.edu>), stored in [Matrix Market][mm] coordinate format and gzipped. The
contents are byte-for-byte as published, including the comment block at the top of each file
that names its contributor and original problem; gzip is only a container. `MatrixMarketFile`
reads them, and `MatrixMarketSolverTests` factors and solves each one with every solver.

`fidapm05`, `spice3f5_matrix01.dat` and `spice3f5_vector01.dat` predate this and are used by
`SparseSolverTests` and `KluSolverTests`. `fidapm05` also comes from the collection.

## License and attribution

The matrices are licensed **CC-BY 4.0**, which is not the MIT license the rest of this
repository uses. The license is satisfied by attribution, which is what this file is for; it
places no condition on the library itself. As the collection asks, the files are redistributed
unmodified.

> Timothy A. Davis and Yifan Hu. *The University of Florida Sparse Matrix Collection.*
> ACM Transactions on Mathematical Software 38, 1, Article 1 (November 2011).
> <https://doi.org/10.1145/2049662.2049663>

> Scott P. Kolodziej et al. *The SuiteSparse Matrix Collection Website Interface.*
> Journal of Open Source Software 4(35), 1244 (2019).
> <https://doi.org/10.21105/joss.01244>

## What is here

The condition numbers are 1-norm estimates and are the reason each matrix is worth keeping:
they set how closely a solution can be pinned down. Everything else is a property of the file.

| File | Group | n | entries | condition | Kind |
|---|---|---:|---:|---:|---|
| `west0067` | HB | 67 | 294 | 3.0e+02 | chemical process separation |
| `Hamrle1` | Hamrle | 32 | 98 | 1.2e+06 | circuit simulation |
| `rajat11` | Rajat | 135 | 812 | 9.4e+05 | circuit simulation |
| `rajat05` | Rajat | 301 | 1384 | 2.2e+05 | circuit simulation |
| `rajat14` | Rajat | 180 | 1503 | 4.2e+08 | circuit simulation |
| `oscil_dcop_01` | Sandia | 430 | 1544 | 1.6e+13 | circuit simulation, DC operating point |
| `fs_760_1` | HB | 760 | 5976 | 8.4e+03 | radiative transfer |
| `rajat19` | Rajat | 1157 | 5399 | 9.2e+10 | circuit simulation |
| `adder_dcop_01` | Sandia | 1813 | 11156 | 1.3e+08 | circuit simulation, DC operating point |
| `add20` | Hamm | 2395 | 17319 | 1.8e+04 | circuit simulation |
| `circuit_2` | Bomhof | 4510 | 21199 | 7.1e+06 | circuit simulation |
| `add32` | Hamm | 4960 | 23884 | 2.1e+02 | circuit simulation |
| `circuit_1` | Bomhof | 2624 | 35823 | 3.3e+05 | circuit simulation |
| `memplus` | Hamm | 17758 | 126150 | 2.7e+05 | circuit simulation |
| `Hamrle2` | Hamrle | 5952 | 22162 | 3.5e+05 | circuit simulation |
| `circuit_3` | Bomhof | 12127 | 48137 | 3.5e+10 | circuit simulation |
| `young1c` | HB | 841 | 4089 | 5.2e+02 | acoustics, **complex** |
| `dwg961b` | Bai | 961 | 10591 | 3.4e+07 | electromagnetics, **complex symmetric** |
| `mhd1280b` | Bai | 1280 | 22778 | 6.0e+12 | magnetohydrodynamics, **complex Hermitian** |
| `qc324` | Bai | 324 | 26730 | 7.4e+04 | electromagnetics, **complex symmetric** |

`entries` counts the mirrored half for the files that only store one triangle, so it is the
number of entries a solver ends up holding rather than the number of lines in the file.

## Adding another one

```
curl -o name.tar.gz https://sparse.tamu.edu/MM/<Group>/<name>.tar.gz
tar xzf name.tar.gz
gzip -9 name/name.mtx
```

Then add a `Case` to `MatrixMarketSolverTests` with the size, the entry count and a condition
estimate, and a row to the table above. The condition estimate only has to be right to an
order of magnitude:

```python
import scipy.io, scipy.sparse, scipy.sparse.linalg, numpy
A = scipy.sparse.csc_matrix(scipy.io.mmread("name.mtx"))
print(numpy.linalg.cond(A.todense(), 1))   # small matrices; estimate larger ones
```

Keep an eye on the size. The whole set here is about 2.3 MB; a single file should stay well
under 10 MB so that a clone does not have to carry it.

[mm]: https://math.nist.gov/MatrixMarket/formats.html
