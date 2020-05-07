﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class LeagueDomesticTierDict
	{
		/// <summary>
		/// LEAGUE DOMESTIC TIER LEVEL AS OF 2020 MAY 06
		/// LEAGUES ONLY (NO CUP COMPETITIONS)
		/// RESERVE AND YOUTH LEAGUES EXCLUDED
		/// INTERNATIONAL LEAGUES EXCLUDED
		/// BRAZILIAN STATE LEAGUES EXCLUDED
		/// SPLIT SEASON (APERTURA-CLAUSURA) SET FOR BOTH HALVES
		/// </summary>
		public static readonly Dictionary<int, int> CurrentDomesticTiers =
			new Dictionary<int, int>
			{
				{689, 1},
				{695, 1},
				{707, 1},
				{735, 1},
				{682, 1},
				{1482, 2},
				{747, 1},
				{701, 1},
				{741, 1},
				{713, 1},
				{722, 1},
				{729, 1},
				{1672, 1},
				{1673, 2},
				{1496, 2},
				{1665, 3},
				{1671, 3},
				{1500, 1},
				{1686, 3},
				{1690, 2},
				{1697, 4},
				{1704, 5},
				{1711, 1},
				{1763, 2},
				{1788, 3},
				{1795, 3},
				{1802, 3},
				{1768, 1},
				{1728, 1},
				{1736, 3},
				{1747, 3},
				{1773, 2},
				{1781, 1},
				{688, 4},
				{694, 4},
				{706, 4},
				{1946, 1},
				{1954, 2},
				{1962, 1},
				{1918, 2},
				{1923, 2},
				{1481, 3},
				{746, 4},
				{2058, 5},
				{2066, 1},
				{2083, 2},
				{2091, 1},
				{2101, 1},
				{2096, 1},
				{1825, 2},
				{700, 1},
				{740, 1},
				{1832, 1},
				{1990, 2},
				{1995, 1},
				{1985, 1},
				{2002, 1},
				{2007, 1},
				{2012, 1},
				{712, 2},
				{1860, 1},
				{1879, 1},
				{1910, 1},
				{1898, 1},
				{2025, 1},
				{2044, 2},
				{1853, 1},
				{2181, 2},
				{728, 1},
				{2049, 2},
				{2054, 1},
				{2122, 2},
				{2138, 3},
				{2206, 4},
				{1841, 2},
				{1846, 3},
				{1582, 1},
				{1491, 1},
				{1659, 1},
				{1495, 2},
				{1664, 3},
				{1499, 1},
				{1715, 1},
				{1719, 2},
				{1679, 1},
				{1685, 2},
				{1689, 1},
				{1696, 1},
				{1703, 1},
				{1710, 1},
				{1757, 1},
				{1787, 1},
				{1814, 3},
				{1806, 3},
				{1930, 1},
				{1945, 2},
				{1953, 1},
				{1961, 1},
				{745, 1},
				{2057, 2},
				{2070, 2},
				{2082, 1},
				{2090, 2},
				{2100, 2},
				{2095, 1},
				{1824, 1},
				{699, 2},
				{739, 1},
				{1831, 2},
				{1989, 1},
				{1994, 1},
				{1984, 1},
				{2006, 1},
				{711, 1},
				{1867, 2},
				{1883, 1},
				{1887, 1},
				{1892, 1},
				{1874, 2},
				{1914, 1},
				{1909, 1},
				{1897, 2},
				{1902, 1},
				{2024, 2},
				{2039, 3},
				{2043, 1},
				{2016, 4},
				{2243, 1},
				{2191, 2},
				{720, 1},
				{1852, 1},
				{2180, 2},
				{2048, 1},
				{2053, 1},
				{2121, 2},
				{2137, 1},
				{2205, 2},
				{2172, 1},
				{1836, 1},
				{1818, 3},
				{1581, 1},
				{1038, 1},
				{1017, 1},
				{1062, 3},
				{1490, 1},
				{1654, 2},
				{1658, 1},
				{1494, 3},
				{1663, 2},
				{1669, 1},
				{1498, 2},
				{1718, 1},
				{1678, 2},
				{1684, 3},
				{1688, 1},
				{1702, 1},
				{1709, 2},
				{1756, 3},
				{1761, 1},
				{1786, 1},
				{1793, 1},
				{1800, 2},
				{1766, 1},
				{1726, 3},
				{1734, 1},
				{1751, 1},
				{1745, 1},
				{1723, 2},
				{1771, 1},
				{1776, 2},
				{1779, 1},
				{1110, 2},
				{686, 1},
				{692, 1},
				{2141, 1},
				{2145, 2},
				{2149, 1},
				{1805, 1},
				{704, 2},
				{1929, 1},
				{1944, 1},
				{1952, 2},
				{1960, 2},
				{1503, 1},
				{1508, 1},
				{732, 2},
				{679, 1},
				{1479, 2},
				{2056, 2},
				{2074, 1},
				{2064, 2},
				{2069, 1},
				{2081, 2},
				{2089, 3},
				{2099, 1},
				{2104, 1},
				{2094, 1},
				{1823, 4},
				{698, 4},
				{738, 4},
				{1830, 4},
				{1988, 4},
				{1993, 4},
				{1983, 3},
				{1979, 3},
				{2000, 2},
				{2005, 1},
				{2010, 2},
				{710, 1},
				{1858, 1},
				{1877, 1},
				{1866, 4},
				{1882, 4},
				{1886, 4},
				{1891, 1},
				{1873, 1},
				{1908, 2},
				{1896, 1},
				{1901, 2},
				{2023, 1},
				{2031, 1},
				{726, 2},
				{2047, 3},
				{2052, 3},
				{1454, 3},
				{2120, 3},
				{2136, 4},
				{2204, 4},
				{2171, 4},
				{1839, 4},
				{1844, 2},
				{1048, 3},
				{1033, 1},
				{1486, 1},
				{1489, 2},
				{1075, 1},
				{1653, 2},
				{1657, 3},
				{1493, 1},
				{1668, 1},
				{1713, 1},
				{1717, 1},
				{1677, 3},
				{1683, 3},
				{1687, 1},
				{1694, 1},
				{1701, 2},
				{1722, 2},
				{1770, 2},
				{1775, 3},
				{1778, 3},
				{1109, 1},
				{685, 3},
				{1535, 3},
				{691, 3},
				{2140, 3},
				{2144, 1},
				{2148, 1},
				{1808, 2},
				{1812, 1},
				{1804, 2},
				{703, 1},
				{1928, 2},
				{1943, 3},
				{1959, 1},
				{1915, 2},
				{678, 2},
				{2055, 1},
				{2073, 2},
				{2098, 2},
				{2103, 1},
				{2093, 2},
				{1822, 2},
				{697, 1},
				{737, 2},
				{1829, 1},
				{1992, 1},
				{1978, 1},
				{1999, 2},
				{2009, 1},
				{709, 1},
				{1605, 2},
				{1857, 1},
				{1876, 2},
				{1865, 2},
				{1881, 3},
				{1885, 2},
				{1890, 1},
				{1872, 4},
				{2018, 4},
				{2241, 4},
				{2189, 4},
				{718, 4},
				{1850, 4},
				{2178, 4},
				{725, 4},
				{2046, 4},
				{2051, 4},
				{1453, 2},
				{2119, 2},
				{2135, 2},
				{2203, 2},
				{2170, 2},
				{1843, 3},
				{1834, 6},
				{1816, 6},
				{192, 6},
				{196, 6},
				{1580, 6},
				{1042, 6},
				{313, 6},
				{1045, 6},
				{1016, 6},
				{1061, 4},
				{1488, 1},
				{1652, 1},
				{497, 3},
				{1712, 1},
				{1707, 3},
				{1754, 3},
				{1759, 4},
				{1784, 4},
				{1791, 4},
				{1798, 4},
				{1764, 4},
				{1732, 4},
				{1749, 4},
				{1743, 4},
				{1721, 4},
				{1769, 4},
				{1108, 2},
				{684, 1},
				{1534, 3},
				{690, 3},
				{2139, 3},
				{2143, 3},
				{2147, 4},
				{1807, 4},
				{1811, 4},
				{1803, 4},
				{1558, 4},
				{702, 4},
				{1568, 4},
				{1934, 4},
				{1938, 4},
				{1927, 4},
				{1942, 4},
				{1950, 4},
				{1958, 4},
				{1294, 4},
				{1919, 4},
				{1501, 4},
				{1506, 4},
				{730, 4},
				{677, 5},
				{1477, 5},
				{1553, 5},
				{742, 5},
				{1651, 5},
				{2072, 5},
				{2062, 5},
				{2067, 5},
				{2079, 5},
				{2087, 5},
				{994, 5},
				{2097, 5},
				{2092, 6},
				{1821, 6},
				{696, 8},
				{736, 8},
				{1828, 8},
				{1986, 8},
				{1991, 8},
				{1981, 8},
				{1977, 7},
				{1998, 7},
				{2003, 7},
				{2008, 1},
				{708, 2},
				{1604, 1},
				{1856, 4},
				{1875, 4},
				{1864, 4},
				{1880, 4},
				{1884, 4},
				{1889, 4},
				{1871, 4},
				{1911, 4},
				{1906, 2},
				{1894, 3},
				{1899, 3},
				{2021, 3},
				{2029, 3},
				{2036, 3},
				{2040, 2},
				{2240, 2},
				{717, 1},
				{2050, 4},
				{1452, 4},
				{2118, 4},
				{600, 4},
				{2134, 4},
				{2202, 4},
				{2113, 4},
				{2169, 4},
				{1837, 4},
				{1833, 2},
				{1026, 4},
				{1047, 4},
				{1032, 3},
				{1022, 3},
				{51, 1},
				{1058, 3},
				{1395, 3},
				{1547, 3},
				{52, 3},
				{53, 3},
				{502, 3},
				{406, 3},
				{1195, 3},
				{409, 3},
				{59, 3},
				{1692, 1},
				{1699, 3},
				{104, 1},
				{868, 1},
				{1002, 1},
				{643, 1},
				{849, 3},
				{180, 3},
				{84, 3},
				{140, 1}
			};
	}
}
