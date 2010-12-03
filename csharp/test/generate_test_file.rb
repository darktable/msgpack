require 'msgpack'

File.open('TestFiles/test.mpac', 'w') do |f| 
  #f.write([0, -123, 3, 12345678].to_msgpack)
  f.write 12345678.to_msgpack
end