require 'msgpack'

File.open('TestFiles/test.mpac', 'wb') do |f| 
  nan = 0.0/0.0
  positive_infinity = 1.0/0.0
  negative_infinity = -1.0/0.0
  
  [0, 1, -1, -123, 12345678, -31, -128, -65535, -(1 << 16), -(1 << 24), 31, 255, 256, (1 << 16), (1 << 24), (1 << 32)].to_msgpack(f)
  [0.0, -0.0, 1.0, -1.0, 3.40282347E+38, -3.40282347E+38, 1.7976931348623157E+308, -1.7976931348623157E+308, nan, positive_infinity, negative_infinity].to_msgpack(f)
  0xffffffffffffffff.to_msgpack(f)
  nil.to_msgpack(f)
  true.to_msgpack(f)
  false.to_msgpack(f)

  (1..65535).to_a.to_msgpack(f)
  (1..66000).to_a.to_msgpack(f)
  
  {1 => true, 10 => false, -127 => true}.to_msgpack(f)
  
  h = {}
  (1..65536).each { |v| h[v] = v*2 }
  h.to_msgpack(f)
  
  {1 => "qwerty", 2 => "zxc", 5 => "", 6 => nil}.to_msgpack(f)

end